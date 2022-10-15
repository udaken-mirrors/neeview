using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 指向性ページ範囲
    /// </summary>
    public struct PageRange : IEquatable<PageRange>, IComparable<PageRange>
    {
        public PageRange()
        {
            this.Position = new PagePosition();
            this.Size = 1;
        }

        public PageRange(PagePosition position, int partSize)
        {
            this.Position = position;
            this.Size = partSize;
        }

        public PageRange(PagePosition position, int partSize, PageReadOrder pageReadOrder)
        {
            if (partSize < 1) throw new ArgumentOutOfRangeException(nameof(partSize));

            this.Position = position;
            this.Size = partSize * pageReadOrder.ToDirection();
        }

        public PageRange(PagePosition position, int direction, int pageSize)
        {
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            var last = new PagePosition(position.Index + direction * (pageSize - 1), direction > 0 ? 1 : 0);
            var partSize = Math.Abs(last.Value - position.Value) + 1;
            this.Position = position;
            this.Size = direction * partSize;
        }

        public PageRange(PagePosition p0, PagePosition p1)
        {
            var direction = p1 < p0 ? -1 : 1;
            var partSize = Math.Abs(p1.Value - p0.Value) + 1;
            this.Position = p0;
            this.Size = direction * partSize;
        }

        public PageRange(PagePosition p0, PagePosition p1, int direction)
        {
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            var min = p0 < p1 ? p0 : p1;
            var max = p0 < p1 ? p1 : p0;
            var partSize = Math.Abs(max.Value - min.Value) + 1;
            this.Position = (direction > 0) ? min : max;
            this.Size = direction * partSize;
        }

        public PageRange(IEnumerable<PagePosition> positions, int direction)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            var min = positions.Min();
            var max = positions.Max();
            var partSize = Math.Abs(max.Value - min.Value) + 1;
            this.Position = (direction > 0) ? min : max;
            this.Size = direction * partSize;
        }

        public PageRange(IEnumerable<PageRange> parts, int direction)
        {
            if (parts == null) throw new ArgumentNullException(nameof(parts));
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            int count = 0;

            var p0 = new PagePosition();
            var p1 = new PagePosition();

            foreach (var part in parts)
            {
                if (part.PartSize <= 0) continue;

                var a0 = part.Position;
                var a1 = part.Position + part.PartSize - 1;

                if (count == 0)
                {
                    p0 = a0;
                    p1 = a1;
                }
                else
                {
                    if (a0.Value < p0.Value) p0 = a0;
                    if (a1.Value > p1.Value) p1 = a1;
                }

                count++;
            }

            var partSize = Math.Abs(p1.Value - p0.Value) + 1;
            this.Position = direction > 0 ? p0 : p1;
            this.Size = direction * partSize;
        }


        public static readonly PageRange Empty = new(PagePosition.Empty, PagePosition.Empty);

        /// <summary>
        /// 範囲開始
        /// </summary>
        public PagePosition Position { get; }

        /// <summary>
        /// パーツサイズ、方向
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// 範囲終了
        /// </summary>
        public PagePosition Last => Position + Direction * (PartSize - 1);

        public PagePosition Min => Direction > 0 ? Position : Last;
        public PagePosition Max => Direction > 0 ? Last : Position;

        /// <summary>
        /// 方向
        /// </summary>
        public int Direction => Size < 0 ? -1 : 1;

        /// <summary>
        /// 方向 (enum)
        /// </summary>
        public PageReadOrder PartOrder => Size < 0 ? PageReadOrder.LeftToRight : PageReadOrder.RightToLeft;

        /// <summary>
        /// パーツサイズ
        /// </summary>
        public int PartSize => Math.Abs(Size);

        /// <summary>
        /// ページサイズ
        /// </summary>
        public int PageSize => Math.Abs(Position.Index - Last.Index) + 1;


        public override string ToString()
        {
            return $"{Position},{Last}";
        }

        public bool IsEmpty()
        {
            return Position.IsEmpty();
        }

        public bool IsContains(PagePosition position)
        {
            if (position.IsEmpty())
            {
                return false;
            }
            if (this.Direction > 0)
            {
                return this.Position <= position && position <= this.Last;
            }
            else
            {
                return this.Last <= position && position <= this.Position;
            }
        }

        public PageRange Add(PagePosition position)
        {
            if (position.IsEmpty() || IsContains(position))
            {
                return this;
            }

            return new PageRange(new List<PagePosition>() { Position, Last, position }, Direction);
        }

        public PageRange Add(PageRange other)
        {
            if (other.IsEmpty())
            {
                return this;
            }

            var points = new List<PagePosition> { this.Min, this.Max, other.Min, other.Max };
            return new PageRange(points, this.Direction);
        }

        public PageRange Add(IEnumerable<PageRange> others)
        {
            var points = new List<PagePosition> { this.Min, this.Max };

            foreach(var other in others.Where(e => !e.IsEmpty()))
            {
                points.Add(other.Min);
                points.Add(other.Max);
            }

            return new PageRange(points, this.Direction);
        }


        public PagePosition Next()
        {
            return Next(this.Direction);
        }

        public PagePosition Previous()
        {
            return Next(this.Direction * -1);
        }

        public PagePosition Next(int direction)
        {
            if (direction != 1 && direction != -1) throw new ArgumentOutOfRangeException(nameof(direction));

            var pos = (this.Direction != direction) ? this.Position : this.Last;
            return pos + direction;
        }

        public PagePosition Move(int delta)
        {
            int direction = delta < 0 ? -1 : 1;
            var pos = (this.Direction != direction) ? this.Last : this.Position;
            var position = new PagePosition(pos.Index + delta, direction > 0 ? 0 : 1);

            if (Math.Abs(delta) == 1)
            {
                var next = Next(direction);
                return Math.Abs(pos.Value - next.Value) < Math.Abs(pos.Value - position.Value) ? next : position;
            }
            else
            {
                return position;
            }
        }

        /// <summary>
        /// パーツ指定を外した、リソースベースのページ範囲を求める
        /// </summary>
        public PageRange Truncate()
        {
            var min = new PagePosition(Min.Index, 0);
            var max = new PagePosition(Max.Index, 0);
            var range = new PageRange(min, max);
            return range;
        }

        /// <summary>
        /// 範囲を制限する
        /// </summary>
        public PageRange Clamp(PagePosition p0, PagePosition p1)
        {
            if (p0.IsEmpty()) throw new ArgumentException("Must not be empty.", nameof(p0));
            if (p1.IsEmpty()) throw new ArgumentException("Must not be empty.", nameof(p1));

            var minLimit = p0 < p1 ? p0 : p1;
            var maxLimit = p0 < p1 ? p1 : p0;

            var min = Min < minLimit ? minLimit : Min;
            var max = Max > maxLimit ? maxLimit : Max;

            return new PageRange(min, max, this.Direction);
        }

        public override bool Equals(object? obj)
        {
            return obj is PageRange range && Equals(range);
        }

        public bool Equals(PageRange other)
        {
            return Position.Equals(other.Position) &&
                   Size == other.Size;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Size);
        }

        public int CompareTo(PageRange other)
        {
            var result = Position.CompareTo(other.Position);
            if (result == 0)
            {
                result = Size.CompareTo(other.Size);
            }
            return result;
        }

        public static bool operator ==(PageRange left, PageRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PageRange left, PageRange right)
        {
            return !(left == right);
        }

        public static bool operator <(PageRange left, PageRange right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(PageRange left, PageRange right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(PageRange left, PageRange right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(PageRange left, PageRange right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
