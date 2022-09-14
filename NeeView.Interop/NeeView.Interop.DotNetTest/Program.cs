using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Interop.DotNetTest
{
    class Program
    {
        private class NativeMethods
        {
            const string NeeViewInteropDll = "NeeView.Interop.dll";

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

            [DllImport(NeeViewInteropDll, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool NVGetImageCodecInfo(uint index, StringBuilder friendryName, StringBuilder fileExtensions);

            [DllImport(NeeViewInteropDll)]
            public static extern void NVCloseImageCodecInfo();

            [DllImport(NeeViewInteropDll)]
            public static extern void NVFpReset();

            [DllImport(NeeViewInteropDll, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool NVGetFullPathFromShortcut([MarshalAs(UnmanagedType.LPWStr)] string shortcut, StringBuilder fullPath);


            static NativeMethods()
            {
                if (!TryLoadNativeLibrary(AppDomain.CurrentDomain.RelativeSearchPath))
                {
                    TryLoadNativeLibrary(Path.GetDirectoryName(typeof(NativeMethods).Assembly.Location));
                }
            }

            private static bool TryLoadNativeLibrary(string path)
            {
                if (path == null)
                {
                    return false;
                }

                path = Path.Combine(path, IntPtr.Size == 4 ? "x86" : "x64");
                path = Path.Combine(path, NeeViewInteropDll);

                Debug.WriteLine($"LoadLibrary: {path}");
                return File.Exists(path) && LoadLibrary(path) != IntPtr.Zero;
            }
        }


        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            NativeMethods.NVFpReset();

            try
            {
                var friendlyName = new StringBuilder(2048);
                var fileExtensions = new StringBuilder(2048);
                for (uint i = 0; NativeMethods.NVGetImageCodecInfo(i, friendlyName, fileExtensions); ++i)
                {
                    Console.WriteLine($"{friendlyName}: {fileExtensions}");
                }
                NativeMethods.NVCloseImageCodecInfo();
            }
            catch (SEHException ex)
            {
                Console.WriteLine("Exception:: " + ex);
                Console.WriteLine($"HRESULT: {ex.ErrorCode:X}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:: " + ex);
            }

            Console.WriteLine($"> time: {sw.ElapsedMilliseconds}ms");

            // Shortcut
            {
                string shortcut1 = "E:\\Work\\Labo\\ショートカットテスト\\1ショート2.lnk";
                string shortcut2 = "E:\\Work\\Labo\\ショートカットテスト\\1ショート—2.lnk";

                var fullpath1 = new StringBuilder(1024);
                NativeMethods.NVGetFullPathFromShortcut(shortcut1, fullpath1);
                Console.WriteLine($"shotcut1: {fullpath1}");

                var fullpath2 = new StringBuilder(1024);
                NativeMethods.NVGetFullPathFromShortcut(shortcut2, fullpath2);
                Console.WriteLine($"shotcut2: {fullpath2}");
            }

            Console.ReadKey();
        }
    }
}
