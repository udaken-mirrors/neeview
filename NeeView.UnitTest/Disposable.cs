using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.UnitTest
{
    public class DisposableTest
    {
        [Fact]
        public void DisposableCollectionTest()
        {
            int count = 0;

            using (var disposables = new DisposableCollection())
            {
                disposables.Add(new AnonymousDisposable(() =>
                {
                    count += 1;
                    Assert.Equal(3, count);
                }));
                disposables.Add(new AnonymousDisposable(() =>
                {
                    count += 2;
                    Assert.Equal(2, count);
                }));

                // Dispose()を何度読んでも大丈夫
                disposables.Dispose(); 
            }

            Assert.Equal(3, count);
        }
    }
}
