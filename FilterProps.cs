using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace gep
{
    class FilterProps : IDisposable
    {
        string name;
        string longname;
        string guid;
        string filename; //with path
        int version;
        int merit;
        public string catguid;
        static string sys32 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.System)+"\\";
        long filesize;
        DateTime cr_time, mod_time;
        FileVersionInfo ver_info;
        bool prepared = false;
        IntPtr pointer = IntPtr.Zero; //to IUnknown

        public FilterProps(string _name, string _longname, string _guid, string _catguid)
        {
            name = _name;
            longname = _longname;
            guid = _guid;            
            catguid = _catguid;
            //MakeFileName();
            //MakeMerit(_catguid);
        }

        public string MakeFileName() //by guid and longname
        {
            filename = null;
            string keyname;
            if (longname!=null) {
                int i = longname.Length - 5;
                if (i>=0 && guid == "{D76E2820-1563-11CF-AC98-00AA004C0FA9}" && longname[i]=='\\') { //VfW codec
                    keyname = @"SYSTEM\CurrentControlSet\Control\MediaResources\icm\VIDC." + longname.Substring(i + 1);
                    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(keyname)) {
                        if (rk != null) filename = rk.GetValue("Driver") as string;                    
                    }
                }
            }

            if (filename==null && guid != null) {
                keyname = @"CLSID\" + guid + @"\InprocServer32";
                using (RegistryKey rk = Registry.ClassesRoot.OpenSubKey(keyname)) {
                    if (rk != null) filename = rk.GetValue("") as string;                    
                }
            }

            if (filename == null)
                filename = "";
            else
                if (!filename.Contains("\\"))
                    filename = sys32 + filename;

            return filename;
        }

        public void Prepare() //filename must be created
        {
            if (prepared)
                return;
            try
            {
                string keyname = @"CLSID\" + catguid + @"\Instance\" + guid;
                using (RegistryKey rk = Registry.ClassesRoot.OpenSubKey(keyname))
                {
                    if (rk != null)
                    {
                        byte[] bytes = (byte[])rk.GetValue("FilterData");
                        if (bytes != null && bytes.Length >= 8)
                        {
                            version = (((((bytes[3] << 8) + bytes[2]) << 8) + bytes[1]) << 8) + bytes[0];
                            merit = (((((bytes[7] << 8) + bytes[6]) << 8) + bytes[5]) << 8) + bytes[4];
                        }
                    }
                }

                try
                {
                    FileInfo fi = new FileInfo(filename);
                    if (fi.Exists)
                    {
                        filesize = fi.Length;
                        cr_time = fi.CreationTime;
                        mod_time = fi.LastWriteTime;
                        ver_info = FileVersionInfo.GetVersionInfo(filename);
                    }
                    else
                        filename += "  (file not found)";
                }
                catch
                {
                    filename += "  (file not found)";
                }
                prepared = true;
            }
            catch (COMException e)
            {
                Graph.ShowCOMException(e, "Can't get filter info");
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Can't get filter info");
                return;
            }
        }

        public void SetFilter(Filter f)
        {
            pointer = Marshal.GetIUnknownForObject(f.BaseFilter);
            Marshal.Release(pointer); // do not keep refcount increased
        }

        [CategoryAttribute("1) Filter"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Friendly name.")]
        public string Name
        {
            get { return name; }
        }

        [CategoryAttribute("1) Filter"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("CLSID (GUID) of this filter.")]
        public string CLSID
        {
            get { return guid; }
        }

        [CategoryAttribute("1) Filter"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Display name.")]
        public string DisplayName
        {
            get { return longname; }
            set { longname = value; }
        }

        [CategoryAttribute("1) Filter"),
           ReadOnlyAttribute(true),
           DescriptionAttribute("Friendly name.")]
        public string FriendlyName
        {
            get { return Filterz.friendlyNames.ContainsKey(guid)? Filterz.friendlyNames[guid] : name; }
        }

        [CategoryAttribute("1) Filter"),
           ReadOnlyAttribute(true),
           DescriptionAttribute("Category name.")]
        public string CategoryName
        {
            get { return Filterz.catnames.ContainsKey(catguid) ? Filterz.catnames[catguid] : "?"; }
        }


        [CategoryAttribute("1) Filter"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Version of the filter.")]
        public string Version
        {
            get { Prepare(); return "0x" + version.ToString("X8"); }
        }

        [CategoryAttribute("1) Filter"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Merit of the filter.")]
        public string Merit
        {
            get { Prepare(); return "0x" + merit.ToString("X8"); }
        }

        [CategoryAttribute("1) Filter"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Pointer to IUnknown.")]
        public string Pointer
        {
            get { Prepare(); return "0x" + pointer.ToString(IntPtr.Size == 4 ? "X8" : "X16"); }
        }

        [CategoryAttribute("2) File"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("File containing this filter.")]
        public string FileName
        {
            get {
                if (filename == null)
                    MakeFileName();                
                return filename!=null && filename.Length > 0 ? 
                    filename.Substring(filename.LastIndexOf('\\') + 1) : ""; 
            }
        }

        [CategoryAttribute("2) File"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Folder with file containing filter.")]
        public string Path
        {
            get {
                if (filename == null)
                    MakeFileName();
                return filename != null && filename.Length > 0 ? 
                    filename.Substring(0, filename.LastIndexOf('\\')) : "";
            }
        }

        [CategoryAttribute("2) File"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Size of file.")]
        public long Size
        {
            get { Prepare(); return filesize; }
        }

        [CategoryAttribute("2) File"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Modification date of file.")]
        public DateTime ModificationDate
        {
            get { Prepare(); return mod_time; }
        }

        [CategoryAttribute("2) File"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Creation date of file.")]
        public DateTime CreationDate
        {
            get { Prepare(); return cr_time; }
        }

        [CategoryAttribute("2) File"),
        ReadOnlyAttribute(true),
        DescriptionAttribute("Expand to see version information."),
        TypeConverterAttribute(typeof(ExpandableObjectConverter))]
        public FileVersionInfo VersionInfo
        {
            get { Prepare(); return ver_info; }
        }

        public int GetMerit() { return merit; }
        public string GetFileName() 
        {
            if (filename == null)
                MakeFileName();
            return filename; 
        }

        public void SetMerit(int mr)
        {
            string keyname = @"CLSID\" + catguid + @"\Instance\" + guid;
            byte[] bytes = null;
            //read data and modify the value
            using (RegistryKey rk = Registry.ClassesRoot.OpenSubKey(keyname, false))
            {
                if (rk != null)
                {
                    bytes = (byte[])rk.GetValue("FilterData");
                    if (bytes != null && bytes.Length >= 8)
                    {
                        //merit = (((((bytes[7] << 8) + bytes[6]) << 8) + bytes[5]) << 8) + bytes[4];
                        bytes[7] = (byte)(mr >> 24);
                        bytes[6] = (byte)((mr >> 16) & 0xff);
                        bytes[5] = (byte)((mr >> 8) & 0xff);
                        bytes[4] = (byte)(mr & 0xff);
                        //rk.SetValue("FilterData", bytes);
                    }
                }
            }
            if (bytes == null) return;

            try // to write to registry
            {
                using (RegistryKey rk = Registry.ClassesRoot.OpenSubKey(keyname, true))
                {
                    if (rk != null)
                        rk.SetValue("FilterData", bytes);
                }
            }
            catch (SecurityException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Windows Registry Editor Version 5.00");
                sb.AppendLine();
                sb.AppendFormat(@"[HKEY_CLASSES_ROOT\CLSID\{0}\Instance\{1}]", catguid, guid);
                sb.AppendLine();
                sb.Append("\"FilterData\"=hex:");
                foreach (byte x in bytes)
                    sb.AppendFormat("{0:X2},", x);

                sb.AppendLine();
                sb.AppendFormat(@"[HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{0}\Instance\{1}]", catguid, guid);
                sb.AppendLine();
                sb.Append("\"FilterData\"=hex:");
                foreach (byte x in bytes)
                    sb.AppendFormat("{0:X2},", x);

                string text = sb.ToString();                
                string path = System.IO.Path.GetTempPath() + "ChangeFilterData.reg";
                var w = new StreamWriter(path, false, Encoding.Unicode);
                w.Write(text);
                w.Close();

                var si = new ProcessStartInfo("regedit.exe", path);
                si.Verb = "runas";
                Process.Start(si).WaitForExit();
            }
        }

        public FilterPropsKernel Kernel()
        {
            return new FilterPropsKernel(name, longname, guid, catguid); 
        }

        public void Dispose()
        {
            //if (pointer != IntPtr.Zero)
            //    Marshal.Release(pointer); //already done after acquiring
            pointer = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        ~FilterProps()
        {
            Dispose();
        }
    }

    class FilterPropsKernel
    {
        string name;
        string longname;
        string guid;
        string catguid;

        public FilterPropsKernel(string _name, string _longname, string _guid, string _catguid)
        {
            name = _name; longname = _longname; guid = _guid; catguid = _catguid;
        }

        public FilterProps MkFilterProps()
        {
            FilterProps fp =  new FilterProps(name, longname, guid, catguid);
            fp.MakeFileName();
            return fp;
        }

        public void SaveTo(List<string> slist)
        {
            slist.Add(name);
            slist.Add(longname);
            slist.Add(guid);
            slist.Add(catguid);
        }

        public static FilterPropsKernel FromList(List<string> slist)
        {
            if (slist.Count < 4) return null;
            FilterPropsKernel fk = new FilterPropsKernel(slist[0], slist[1], slist[2], slist[3]);
            slist.RemoveRange(0, 4);
            return fk;
        }
    }


}
