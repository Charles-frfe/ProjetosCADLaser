using System;
using System.Runtime.InteropServices;

namespace ProjetosCADLaser.Utilities
{
    public static class FolderPicker
    {
        public static string Selecionar(
            IntPtr janelaPai,
            string titulo,
            string pastaInicial = null)
        {
            var dialogo = (IFileDialog)new FileOpenDialog();

            try
            {
                FileOpenOptions opcoes;
                dialogo.GetOptions(out opcoes);

                opcoes |= FileOpenOptions.PickFolders;
                opcoes |= FileOpenOptions.ForceFileSystem;
                opcoes |= FileOpenOptions.PathMustExist;

                dialogo.SetOptions(opcoes);

                if (!string.IsNullOrWhiteSpace(titulo))
                    dialogo.SetTitle(titulo);

                if (!string.IsNullOrWhiteSpace(pastaInicial))
                {
                    IShellItem pasta;

                    var guid = typeof(IShellItem).GUID;

                    if (SHCreateItemFromParsingName(
                        pastaInicial,
                        IntPtr.Zero,
                        ref guid,
                        out pasta) == 0)
                    {
                        dialogo.SetFolder(pasta);
                    }
                }

                var resultado = dialogo.Show(janelaPai);

                // Cancelado pelo usuário
                if (resultado == unchecked((int)0x800704C7))
                    return null;

                if (resultado != 0)
                    Marshal.ThrowExceptionForHR(resultado);

                IShellItem item;
                dialogo.GetResult(out item);

                IntPtr caminhoPtr;
                item.GetDisplayName(
                    ShellItemDisplayName.FileSystemPath,
                    out caminhoPtr);

                try
                {
                    return Marshal.PtrToStringUni(caminhoPtr);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(caminhoPtr);
                }
            }
            finally
            {
                if (dialogo != null &&
                    Marshal.IsComObject(dialogo))
                {
                    Marshal.FinalReleaseComObject(dialogo);
                }
            }
        }

        [DllImport(
            "shell32.dll",
            CharSet = CharSet.Unicode,
            PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

        [ComImport]
        [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        private class FileOpenDialog
        {
        }

        [ComImport]
        [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileDialog
        {
            [PreserveSig]
            int Show(IntPtr parent);

            void SetFileTypes(
                uint cFileTypes,
                IntPtr rgFilterSpec);

            void SetFileTypeIndex(uint iFileType);

            void GetFileTypeIndex(out uint piFileType);

            void Advise(IntPtr pfde, out uint pdwCookie);

            void Unadvise(uint dwCookie);

            void SetOptions(FileOpenOptions fos);

            void GetOptions(out FileOpenOptions pfos);

            void SetDefaultFolder(IShellItem psi);

            void SetFolder(IShellItem psi);

            void GetFolder(out IShellItem ppsi);

            void GetCurrentSelection(out IShellItem ppsi);

            void SetFileName(
                [MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetFileName(
                [MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            void SetTitle(
                [MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            void SetOkButtonLabel(
                [MarshalAs(UnmanagedType.LPWStr)] string pszText);

            void SetFileNameLabel(
                [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            void GetResult(out IShellItem ppsi);

            void AddPlace(
                IShellItem psi,
                int fdap);

            void SetDefaultExtension(
                [MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            void Close(int hr);

            void SetClientGuid(ref Guid guid);

            void ClearClientData();

            void SetFilter(IntPtr pFilter);
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(
                IntPtr pbc,
                ref Guid bhid,
                ref Guid riid,
                out IntPtr ppv);

            void GetParent(out IShellItem ppsi);

            void GetDisplayName(
                ShellItemDisplayName sigdnName,
                out IntPtr ppszName);

            void GetAttributes(
                uint sfgaoMask,
                out uint psfgaoAttribs);

            void Compare(
                IShellItem psi,
                uint hint,
                out int piOrder);
        }

        [Flags]
        private enum FileOpenOptions : uint
        {
            PathMustExist = 0x00000800,
            PickFolders = 0x00000020,
            ForceFileSystem = 0x00000040
        }

        private enum ShellItemDisplayName : uint
        {
            FileSystemPath = 0x80058000
        }
    }
}