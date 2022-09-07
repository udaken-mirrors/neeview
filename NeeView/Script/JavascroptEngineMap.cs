using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// スレッドIDとエンジンの対応表
    /// </summary>
    public class JavascroptEngineMap
    {
        static JavascroptEngineMap() => Current = new JavascroptEngineMap();
        public static JavascroptEngineMap Current { get; }


        private readonly ConcurrentDictionary<int, JavascriptEngine> _map = new();


        private JavascroptEngineMap()
        {
        }


        public void Add(JavascriptEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            int id = System.Environment.CurrentManagedThreadId;
            //Debug.WriteLine($"> JavascriptEngine.{id}: add");
            Debug.Assert(!_map.ContainsKey(id));
            var result = _map.TryAdd(id, engine);
            Debug.Assert(result);
        }

        public void Remove(JavascriptEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            int id = System.Environment.CurrentManagedThreadId;
            //Debug.WriteLine($"> JavascriptEngine.{id}: remove");
            var result = _map.TryRemove(id, out var target);
            Debug.Assert(result && target == engine);
        }

        public JavascriptEngine GetCurrentEngine()
        {
            int id = System.Environment.CurrentManagedThreadId;
            //Debug.WriteLine($"> JavascriptEngine.{id}: access");
            _map.TryGetValue(id, out var engine);
            Debug.Assert(engine != null);
            return engine;
        }

    }

}
