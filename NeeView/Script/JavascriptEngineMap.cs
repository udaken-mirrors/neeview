using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// スレッドIDとエンジンの対応表
    /// </summary>
    public class JavaScriptEngineMap
    {
        static JavaScriptEngineMap() => Current = new JavaScriptEngineMap();
        public static JavaScriptEngineMap Current { get; }


        private readonly ConcurrentDictionary<int, JavaScriptEngine> _map = new();


        private JavaScriptEngineMap()
        {
        }


        public void Add(JavaScriptEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            int id = System.Environment.CurrentManagedThreadId;
            //Debug.WriteLine($"> JavaScriptEngine.{id}: add");
            Debug.Assert(!_map.ContainsKey(id));
            var result = _map.TryAdd(id, engine);
            Debug.Assert(result);
        }

        public void Remove(JavaScriptEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            int id = System.Environment.CurrentManagedThreadId;
            //Debug.WriteLine($"> JavaScriptEngine.{id}: remove");
            var result = _map.TryRemove(id, out var target);
            Debug.Assert(result && target == engine);
        }

        public JavaScriptEngine GetCurrentEngine()
        {
            int id = System.Environment.CurrentManagedThreadId;
            //Debug.WriteLine($"> JavaScriptEngine.{id}: access");
            _map.TryGetValue(id, out var engine);
            Debug.Assert(engine != null);
            return engine;
        }

    }

}
