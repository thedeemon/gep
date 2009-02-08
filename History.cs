using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.Win32;
using DirectShowLib;
using System.Reflection;

namespace gep
{
    abstract class HistoryItem
    {
        public abstract string Define(CodeGenBase cg);
        public abstract string Build(CodeGenBase cg, Graph graph);

        public string var, clsname, RealName, Name;
        public string srcFileName, dstFileName;
    }

    abstract class CodeGenBase
    {
        public abstract string DefineAddFilterDS(HIAddFilterDS hi);
        public abstract string DefineSetFormat(HISetFormat hi);
        public abstract string DefineConnect(HIConnect hi);
        public abstract string BuildAddFilterDS(HIAddFilterDS hi, Graph graph);
        public abstract string BuildAddFilterMon(HIAddFilterMon hi, Graph graph);
        public abstract string SetFormat(HISetFormat hi);
        protected abstract string SetSampleGrabberMediaType(AMMediaType mt, HistoryItem hi);

        public string Connect(HIConnect hi)
        {
            HistoryItem h1 = history.FindByRealName(hi.filter1);
            HistoryItem h2 = history.FindByRealName(hi.filter2);
            string var1 = (h1 != null) ? h1.var : "?";
            string var2 = (h2 != null) ? h2.var : "?";
            string pair = h1.Name + " and " + h2.Name;
            return directConnect ? connectDirectTpl.GenerateWith(new string[] {
                   "$pair", pair, "$majortype", hi.majortype, "$var1", var1, "$var2", var2, "$pin1", hi.pin1, "$pin2", hi.pin2
                   }) 
                   : connectTpl.GenerateWith(new string[] {
                   "$pair", pair, "$majortype", hi.majortype, "$var1", var1, "$var2", var2
                   });
        }

        public abstract string GenCode(bool useDirectConnect, Graph graph);

        public bool needCreateFilterProc = false, needGetPin = false, directConnect = true;
        public History History { set { history = value; } }

        public CodeSnippet[] snippets;
        public void SaveTemplates()
        {
            if (snippets == null) return;
            string keyname = Program.mainform.keyname;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyname, true);
            if (rk == null)
                rk = Registry.CurrentUser.CreateSubKey(keyname);
            if (rk != null)
            {
                string lang = ToString()+".";
                foreach(CodeSnippet snp in snippets)
                    rk.SetValue(lang + snp.Codename, snp.Text);                
            }
            rk.Close();            
        }

        public void LoadTemplates()
        {
            string keyname = Program.mainform.keyname;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyname);
            if (rk == null)
                return;
            string lang = ToString() + ".";
            foreach (CodeSnippet snp in snippets)
            {
                string s = (string)rk.GetValue(lang + snp.Codename);
                if (s != null) snp.Text = s;
            }
            rk.Close();
        }

        protected History history;
        protected Dictionary<string, string> known = new Dictionary<string, string>(); // guid => CLSID_Shit
        protected List<string> srcFileNames = new List<string>();
        protected List<string> dstFileNames = new List<string>();
        protected CodeSnippet insertTpl, setSrcFileTpl, setDstFileTpl, connectTpl, connectDirectTpl;

        protected string Insert(HistoryItem hi, Graph graph)
        {
            string ins = insertTpl.GenerateWith(new string[] {
                "$var", hi.var, "$name", hi.Name
            });
            string setfname = "";
            if (hi.srcFileName != null)
            {
                srcFileNames.Add(hi.srcFileName);
                int n = srcFileNames.Count;
                setfname = setSrcFileTpl.GenerateWith(new string[] {
                    "$srcvar", hi.var + "_src",  "$var", hi.var, "$filename", "srcFile" + n
                });
            }
            if (hi.dstFileName != null)
            {
                dstFileNames.Add(hi.dstFileName);
                int n = dstFileNames.Count;
                setfname = setDstFileTpl.GenerateWith(new string[] {
                    "$dstvar", hi.var + "_sink",  "$var", hi.var, "$filename", "dstFile" + n
                });
            }

            string setmtype = "";
            if (hi.RealName != null)
            {
                AMMediaType mt = graph.SampleGrabberMediaType(hi.RealName);
                if (mt != null)
                {
                    setmtype = SetSampleGrabberMediaType(mt, hi);
                    DsUtils.FreeAMMediaType(mt);
                }
            }

            return ins + setfname + setmtype + "\r\n";
        }



        protected static string xguid(string guid)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("0x"); sb.Append(guid.Substring(1, 8));
            sb.Append(", 0x"); sb.Append(guid.Substring(10, 4));
            sb.Append(", 0x"); sb.Append(guid.Substring(15, 4));
            sb.Append(", 0x"); sb.Append(guid.Substring(20, 2));
            sb.Append(", 0x"); sb.Append(guid.Substring(22, 2));
            sb.Append(", 0x"); sb.Append(guid.Substring(25, 2));
            sb.Append(", 0x"); sb.Append(guid.Substring(27, 2));
            sb.Append(", 0x"); sb.Append(guid.Substring(29, 2));
            sb.Append(", 0x"); sb.Append(guid.Substring(31, 2));
            sb.Append(", 0x"); sb.Append(guid.Substring(33, 2));
            sb.Append(", 0x"); sb.Append(guid.Substring(35, 2));
            return sb.ToString();
        }

        public abstract string MediaTypeToString(Guid guid);
    }


    class HIAddFilterDS : HistoryItem
    {
        public string CLSID, FileName;
        public HIAddFilterDS(string _Name, string _CLSID, string realName, string _FileName,
            string SrcFileName, string DstFileName, History h)
        {
            Name = _Name; CLSID = _CLSID; RealName = realName; FileName = _FileName;
            srcFileName = SrcFileName; dstFileName = DstFileName; 
            h.MakeVarName(_Name, out clsname, out var);
        }

        public override string Define(CodeGenBase cg)
        {
            return cg.DefineAddFilterDS(this);
        }

        public override string Build(CodeGenBase cg, Graph graph)
        {
            return cg.BuildAddFilterDS(this, graph);
        }
    }

    class HIAddFilterMon : HistoryItem
    {
        public string DisplayName;

        public HIAddFilterMon(string _Name, string _DisplayName, string realname,
            string SrcFileName, string DstFileName, History h)
        {
            Name = _Name; DisplayName = _DisplayName; RealName = realname;
            srcFileName = SrcFileName; dstFileName = DstFileName;
            h.MakeVarName(_Name, out clsname, out var);
        }

        public override string Define(CodeGenBase cg)
        {
            cg.needCreateFilterProc = true;
            return "";
        }

        public override string Build(CodeGenBase cg, Graph graph)
        {
            return cg.BuildAddFilterMon(this, graph);
        }
    }

    class HIConnect : HistoryItem
    {
        public string filter1, filter2, pin1, pin2;
        public int weight = 1;
        public string majortype;
        public Guid majortypeguid;

        public HIConnect(string _filter1, string _pin1, string _filter2, string _pin2, Guid type)
        {
            filter1 = _filter1; filter2 = _filter2; pin1 = _pin1; pin2 = _pin2;
            majortypeguid = type;
        }

        public override string Define(CodeGenBase cg)
        {
            majortype = cg.MediaTypeToString(majortypeguid);
            return cg.DefineConnect(this);
        }

        public override string Build(CodeGenBase cg, Graph graph)
        {
            return cg.Connect(this);
        }

    }

    class HISetFormat : HistoryItem
    {
        public AMMediaType mt;
        public string filter, pin;

        public HISetFormat(string _filter, string _pin, AMMediaType _mt, History h)
        {
            filter = _filter; pin = _pin; mt = _mt;
            string dummy;
            h.MakeVarName("mt", out dummy, out var);  
        }

        public override string Define(CodeGenBase cg)
        {
            cg.needGetPin = true;
            return cg.DefineSetFormat(this);
        }

        public override string Build(CodeGenBase cg, Graph graph)
        {
            return cg.SetFormat(this);
        }
    }

    
    class History
    {
        List<HistoryItem> history = new List<HistoryItem>();
        List<TempHistoryItem> temp = new List<TempHistoryItem>();

        public IEnumerable Items
        {
            get
            {
                foreach (HistoryItem i in history)
                    yield return i;
            }
        }

        public HistoryItem FindByRealName(string realname) //finds a filter
        {
            foreach (HistoryItem i in history)
                if (i.RealName == realname)
                    return i;
            foreach (HistoryItem i in TempHistoryItem.hitems(temp))
                if (i.RealName == realname)
                    return i;
            return null;
        }

        public void AddFilter(FilterProps fp, string realname, string srcFileName, string dstFileName)
        {
            if (fp.DisplayName.StartsWith("@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}")) //DirectShow filters
                history.Add(new HIAddFilterDS(fp.FriendlyName, fp.CLSID, realname, fp.FileName, srcFileName, dstFileName, this));
            else
                history.Add(new HIAddFilterMon(fp.Name, fp.DisplayName, realname, srcFileName, dstFileName, this));
        }

        public void AddFilterIfNew(FilterProps fp, string realname, string srcfilename, string dstfilename, Filter f)
        {
            if (FindByRealName(realname) == null)
                temp.Add(new THIAddFilterDS(
                    new HIAddFilterDS(fp.FriendlyName, fp.CLSID, realname, fp.FileName, srcfilename, dstfilename, this),
                    f));
        }

        /*public void Connect(Pin p1, Pin p2) //not used?
        {
            history.Add(new HIConnect(p1.Filter.Name, p1.Name, p2.Filter.Name, p2.Name));
        }*/

        public void ConnectIfNew(Pin p1, Pin p2, PinConnection con)
        {
            AMMediaType mt = new AMMediaType();
            p1.IPin.ConnectionMediaType(mt);
            Guid type = mt.majorType;
            DsUtils.FreeAMMediaType(mt);

            if (!HasConnection(p1,p2, history) && !HasConnection(p1,p2, TempHistoryItem.hitems(temp)))
                temp.Add(new THIConnect(
                    new HIConnect(p1.Filter.Name, p1.Name, p2.Filter.Name, p2.Name, type), con));
        }

        public void RemoveConnection(Pin p1, Pin p2)
        {
            int i = FindConnection(p1, p2, history);
            if (i >= 0) history.RemoveAt(i);
        }

        int FindConnection(Pin p1, Pin p2, List<HistoryItem> lst)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                HistoryItem hi = lst[i];
                HIConnect hc = hi as HIConnect;
                if (hc != null && hc.filter1 == p1.Filter.Name && hc.filter2 == p2.Filter.Name
                               && hc.pin1 == p1.Name && hc.pin2 == p2.Name)
                    return i;
            }
            return -1;
        }

        static bool HasConnection(Pin p1, Pin p2, IEnumerable<HistoryItem> lst)
        {
            foreach (HistoryItem hi in lst)
            {
                HIConnect hc = hi as HIConnect;
                if (hc != null && hc.filter1 == p1.Filter.Name && hc.filter2 == p2.Filter.Name
                               && hc.pin1 == p1.Name && hc.pin2 == p2.Name)
                    return true;
            }
            return false;
        }

        public void RemoveFilter(string realname) //deletes filter with given realname and all setformats on it
        {
            history.RemoveAll(delegate(HistoryItem hi)
                                  {
                                      if (hi is HISetFormat) return ((HISetFormat) hi).filter == realname;
                                      return hi.RealName == realname;
                                  });
        }

        Dictionary<string, int> vars = new Dictionary<string, int>();

        public void MakeVarName(string fltname, out string clsname, out string var)
        {
            StringBuilder sb1 = new StringBuilder();
            foreach (char c in fltname)
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                    sb1.Append(c);
            string s = sb1.ToString();
            clsname = "CLSID_" + s;
            var = "p" + s;
            if (vars.ContainsKey(var))
            {
                int k = vars[var] + 1;
                vars[var] = k;
                var += k.ToString();
            }
            else
                vars[var] = 1;
        }

        

        public void SetFormat(Pin pin, AMMediaType mt)
        {
            history.Add(new HISetFormat(pin.Filter.Name, pin.Name, mt, this));
        }

        public void CommitAdded(Graph graph) //sort items in temp and add to history
        {
            temp.ForEach(delegate(TempHistoryItem thi) { thi.CalcOrder(); });
            temp.Sort();
            history.AddRange(TempHistoryItem.hitems(temp));
            temp.Clear();
        }

    }


    class CodeGenCPP : CodeGenBase
    {
        CodeSnippet addFiltMonTpl, defineDsTpl, addFiltDsTpl, checkTpl;
        
        public CodeGenCPP()
        {
            InitGuidsTable();
            defineDsTpl = new CodeSnippet("Define CLSID for a custom DirectShow filter", "defineDsTpl",
                "// $guid\r\n" +
                "DEFINE_GUID($clsname,\r\n$xguid); //$file\r\n\r\n",
                "$clsname - name for CLSID value\r\n" +
                "$guid - GUID digits\r\n" +
                "$xguid - GUID as a sequence of 0x.. values\r\n" +
                "$file - name of file containing the filter\r\n");
            defineDsTpl.SetVars(new string[] {
                "$clsname", "CLSID_DivXDecoderFilter",
                "$guid", "{78766964-0000-0010-8000-00AA00389B71}",
                "$xguid", "0x78766964, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71",
                "$file", "divxdec.ax"
            });

            addFiltDsTpl = new CodeSnippet("Create a DirectShow filter", "addFiltDsTpl",
                "    //add $name\r\n" +
                "    CComPtr<IBaseFilter> $var;\r\n"+
                "    hr = $var.CoCreateInstance($clsname);\r\n"+
                "    CHECK_HR(hr, \"Can't create $name\");\r\n",
                "$name - name of the filter\r\n" +
                "$var - variable to hold IBaseFilter\r\n" +
                "$clsname - name of CLSID value for this filter");
            addFiltDsTpl.SetVars(new string[] {
                "$name", "DV Splitter",
                "$var", "pDVSplitter",
                "$clsname", "CLSID_DVSplitter"
            });
            
            insertTpl = new CodeSnippet("Insert filter to graph", "insertTpl",
                "    hr = pGraph->AddFilter($var, L\"$name\");\r\n" +
                "    CHECK_HR(hr, \"Can't add $name to graph\");\r\n",
                "$var - variable holding IBaseFilter\r\n" +
                "$name - name of the filter");
            insertTpl.SetVars(new string[] {
                "$name", "DivX Decoder Filter",
                "$var", "pDivXDecoderFilter"                
            });
            
            setSrcFileTpl = new CodeSnippet("Set source file to IFileSourceFilter", "setSrcFileTpl",
                "    //set source filename\r\n" +
                "    CComQIPtr<IFileSourceFilter, &IID_IFileSourceFilter> $srcvar($var);\r\n" +
                "    if (!$srcvar)\r\n" +
                "        CHECK_HR(E_NOINTERFACE, \"Can't get IFileSourceFilter\");\r\n" +
                "    hr = $srcvar->Load($filename, NULL);\r\n" +
                "    CHECK_HR(hr, \"Can't load file\");\r\n",
                "$srcvar - variable to hold IFileSourceFilter\r\n" +
                "$var - variable holding IBaseFilter\r\n" +
                "$filename - variable holding name of file to open");
            setSrcFileTpl.SetVars(new string[] {
                "$srcvar", "pFileSourceAsync_src",
                "$var", "pFileSourceAsync",
                "$filename", "srcFile1"
            });
            
            setDstFileTpl = new CodeSnippet("Set destination file to IFileSinkFilter", "setDstFileTpl",
                "    //set destination filename\r\n" +
                "    CComQIPtr<IFileSinkFilter, &IID_IFileSinkFilter> $dstvar($var);\r\n" +
                "    if (!$dstvar)\r\n" +
                "        CHECK_HR(E_NOINTERFACE, \"Can't get IFileSinkFilter\");\r\n" +
                "    hr = $dstvar->SetFileName($filename, NULL);\r\n" +
                "    CHECK_HR(hr, \"Can't set filename\");\r\n",
                "$dstvar - variable to hold IFileSinkFilter\r\n" +
                "$var - variable holding IBaseFilter\r\n" +
                "$filename - variable holding name of file to open");
            setDstFileTpl.SetVars(new string[] {
                "$dstvar", "pFilewriter_sink",
                "$var", "pFilewriter",
                "$filename", "dstFile1"
            });
            
            addFiltMonTpl = new CodeSnippet("Create a filter by display name", "addFiltMonTpl",
                "    //add $name\r\n" +
                "    CComPtr<IBaseFilter> $var = CreateFilter(L\"$displayname\");\r\n",
                "$name - name of the filter\r\n" +
                "$var - variable holding IBaseFilter\r\n" +
                "$displayname - display name of the filter\r\n");
            addFiltMonTpl.SetVars(new string[] {
                "$name", "Fraps Video Decompressor",
                "$var", "pFrapsVideoDecompressor",
                "$displayname", @"@device:cm:{33D9A760-90C8-11D0-BD43-00A0C911CE86}\\fps1"
            });
            
            connectTpl = new CodeSnippet("Connect two filters", "connectTpl",
                "    //connect $pair\r\n" +
                "    hr = pBuilder->RenderStream(NULL, &$majortype, $var1, NULL, $var2);\r\n" +
                "    CHECK_HR(hr, \"Can't connect $pair\");\r\n\r\n",
                "$pair - names of connecting filters\r\n" +
                "$majortype - major media type of connection\r\n" +
                "$var1, $var2 - variables holding IBaseFilter of connecting filters");
            connectTpl.SetVars(new string[] {
                "$pair", "File Source (Async.) and AVI Splitter",
                "$majortype", "MEDIATYPE_Stream",
                "$var1", "pFileSourceAsync",
                "$var2", "pAVISplitter"
            });

            connectDirectTpl = new CodeSnippet("Connect two filters directly", "connectDirectTpl",
                "    //connect $pair\r\n" +
                "    hr = pGraph->ConnectDirect(GetPin($var1, L\"$pin1\"), GetPin($var2, L\"$pin2\"), NULL);\r\n" +
                "    CHECK_HR(hr, \"Can't connect $pair\");\r\n\r\n",
                "$pair - names of connecting filters\r\n" +
                "$var1, $var2 - variables holding IBaseFilter of connecting filters\r\n"+
                "$pin1, $pin2 - names of connecting pins");
            connectDirectTpl.SetVars(new string[] {
                "$pair", "File Source (Async.) and AVI Splitter",
                "$var1", "pFileSourceAsync",
                "$var2", "pAVISplitter",
                "$pin1", "Output",
                "$pin2", "input pin"
            });

            checkTpl = new CodeSnippet("Check HRESULT for errors", "checkTpl",
            "BOOL hrcheck(HRESULT hr, TCHAR* errtext)\r\n" +
            "{\r\n" +
            "    if (hr >= S_OK)\r\n" +
            "        return FALSE;\r\n" +
            "    TCHAR szErr[MAX_ERROR_TEXT_LEN];\r\n" +
            "    DWORD res = AMGetErrorText(hr, szErr, MAX_ERROR_TEXT_LEN);\r\n" +
            "    if (res)\r\n" +
            "        printf(\"Error %x: %s\\n%s\\n\",hr, errtext,szErr);\r\n" +
            "    else\r\n" +
            "        printf(\"Error %x: %s\\n\", hr, errtext);\r\n" +
            "    return TRUE;\r\n" +
            "}\r\n\r\n" +            
            "//change this macro to fit your style of error handling\r\n" +
            "#define CHECK_HR(hr, msg) if (hrcheck(hr, msg)) return hr;\r\n" 
            , "");

            snippets = new CodeSnippet[] { 
                defineDsTpl, addFiltDsTpl, addFiltMonTpl, setSrcFileTpl, setDstFileTpl,
                insertTpl, connectTpl, connectDirectTpl, checkTpl                
            };

            LoadTemplates();
            
        }

        public override string ToString()
        {
            return "C++";
        }

        private int lastguidnum = 0;
        Dictionary<string, string> guidnames = new Dictionary<string, string>();

        string GuidName(string guid) // guid must be "{123.."
        {
            if (guidnames.ContainsKey(guid))
                return guidnames[guid];
            lastguidnum++;
            string s = string.Format("GUID{0}", lastguidnum);
            guidnames.Add(guid, s);
            return s;
        }

        public override string DefineAddFilterDS(HIAddFilterDS hi)
        {            
            string guid = hi.CLSID, s;
            if (known.TryGetValue(guid, out s))
            {
                hi.clsname = s;
                return ""; //no need to define
            }
            return defineDsTpl.GenerateWith(new string[] {
                "$clsname", hi.clsname, "$guid", hi.CLSID, "$file", hi.FileName, "$xguid", xguid(guid)
            });
        }

        string DefineIfNotKnown(string guidname, string known_prefix, string guid)
        {
            if (guidname.StartsWith(known_prefix)) return "";
            return defineDsTpl.GenerateWith(new string[] {
                "$clsname", guidname, "$guid", guid, "$file", "", "$xguid", xguid(guid)
            }); 
        }

        public override string DefineSetFormat(HISetFormat hi)
        {
            return DefineIfNotKnown(MediaSubTypeToString(hi.mt.subType), "MEDIASUBTYPE", Graph.GuidToString(hi.mt.subType)) +
                DefineIfNotKnown(MediaTypeToString(hi.mt.majorType), "MEDIATYPE", Graph.GuidToString(hi.mt.majorType));
        }

        public override string DefineConnect(HIConnect hi)
        {
            return DefineIfNotKnown(hi.majortype, "MEDIATYPE", Graph.GuidToString(hi.majortypeguid));
        }

        public override string BuildAddFilterDS(HIAddFilterDS hi, Graph graph)
        {
            return addFiltDsTpl.GenerateWith(new string[] {
                    "$name", hi.Name, "$var", hi.var, "$clsname", hi.clsname
                }) + Insert(hi, graph);
        }

        public override string BuildAddFilterMon(HIAddFilterMon hi, Graph graph)
        {
            return addFiltMonTpl.GenerateWith(new string[] {
                "$name", hi.Name, "$var", hi.var, "$displayname", hi.DisplayName.Replace("\\","\\\\")
            }) + Insert(hi, graph);
        }

        void CreateMediaType(string var, AMMediaType mt, StringBuilder sb)
        {
            sb.AppendFormat("    AM_MEDIA_TYPE {0};\r\n", var);
            sb.AppendFormat("    ZeroMemory(&{0}, sizeof(AM_MEDIA_TYPE));\r\n", var);
            sb.AppendFormat("    {0}.majortype = {1};\r\n", var, MediaTypeToString(mt.majorType));
            sb.AppendFormat("    {0}.subtype = {1};\r\n", var, MediaSubTypeToString(mt.subType));
            string fmtype = CodeSnippet.Translate("FORMAT_" + DsToString.MediaFormatTypeToString(mt.formatType), new string[] {
                "WaveEx", "WaveFormatEx",
                "Mpeg", "MPEG",
                "FORMAT_Null", "GUID_NULL"
            });
            sb.AppendFormat("    {0}.formattype = {1};\r\n", var, fmtype);
            sb.AppendFormat("    {0}.bFixedSizeSamples = {1};\r\n", var, mt.fixedSizeSamples.ToString().ToUpperInvariant());
            sb.AppendFormat("    {0}.cbFormat = {1};\r\n", var, mt.formatSize);
            sb.AppendFormat("    {0}.lSampleSize = {1};\r\n", var, mt.sampleSize);
            sb.AppendFormat("    {0}.bTemporalCompression = {1};\r\n", var, mt.temporalCompression.ToString().ToUpperInvariant());
            string fmtvar = var.Replace("pmt", "format");
            MediaTypeProps mtp = MediaTypeProps.CreateMTProps(mt);
            string fmt_struct = mtp.FormatClass().ToUpperInvariant();
            sb.AppendFormat("    {1} {0};\r\n", fmtvar, fmt_struct);
            sb.AppendFormat("    ZeroMemory(&{0}, sizeof({1}));\r\n", fmtvar, fmt_struct);
            mtp.IterFormatFields(false, delegate(string fldname, string fldvalue)
                {
                    if (!fldvalue.StartsWith("new"))
                        sb.AppendFormat("    {0}.{1} = {2};\r\n", fmtvar, CodeSnippet.Translate(fldname, dict), fldvalue);
                });
            sb.AppendFormat("    {0}.pbFormat = (BYTE*)&{1};\r\n", var, fmtvar);
        }

        public override string SetFormat(HISetFormat hi)
        {
            HistoryItem fhi = history.FindByRealName(hi.filter);
            string fvar = (fhi != null) ? fhi.var : "?";
            StringBuilder sb = new StringBuilder();
            CreateMediaType(hi.var, hi.mt, sb);
            string iscvar = hi.var.Replace("pmt", "isc");
            sb.AppendFormat("    CComQIPtr<IAMStreamConfig, &IID_IAMStreamConfig> {0}(GetPin({1}, L\"{2}\"));\r\n", iscvar, fvar, hi.pin);
            sb.AppendFormat("    hr = {1}->SetFormat(&{0});\r\n", hi.var, iscvar);
            sb.Append("    CHECK_HR(hr, \"Can't set format\");\r\n");
            sb.AppendLine();
            return sb.ToString();
        }

        protected override string SetSampleGrabberMediaType(AMMediaType mt, HistoryItem hi)
        {
            string mtvar = hi.var + "_pmt";
            StringBuilder sb = new StringBuilder();
            CreateMediaType(mtvar, mt, sb);
            string isgvar = mtvar.Replace("pmt", "isg");
            sb.AppendFormat("    CComQIPtr<ISampleGrabber, &IID_ISampleGrabber> {0}({1});\r\n", isgvar, hi.var);
            sb.AppendFormat("    hr = {1}->SetMediaType({0});\r\n", mtvar, isgvar);
            sb.Append("    CHECK_HR(hr, \"Can't set media type to sample grabber\");\r\n");
            sb.AppendLine();
            return sb.ToString();
        }

        readonly string[] dict = new string[] {
                    "AvgTimePerFrame", "AvgTimePerFrame", //vih2
                    "BitErrorRate", "dwBitErrorRate",
                    "BitRate", "dwBitRate",
                    "BmiHeader", "bmiHeader",
                    "ControlFlags", "dwControlFlags",
                    "CopyProtectFlags", "dwCopyProtectFlags",
                    "InterlaceFlags", "dwInterlaceFlags",
                    "PictAspectRatioX", "dwPictAspectRatioX",
                    "PictAspectRatioY", "dwPictAspectRatioY",
                    "Reserved2", "dwReserved2",
                    "SrcRect", "rcSource",
                    "TargetRect", "rcTarget",

                    "BitCount", "biBitCount", //bmih
                    "ClrImportant", "biClrImportant",
                    "ClrUsed", "biClrUsed",
                    "Compression", "biCompression",
                    "Height", "biHeight",
                    ".ImageSize", ".biSizeImage",
                    "Planes", "biPlanes",
                    ".Size", ".biSize",
                    "Width", "biWidth",
                    "XPelsPerMeter", "biXPelsPerMeter",
                    "YPelsPerMeter", "biYPelsPerMeter"  
            };

        /*KeyValuePair<string, string> FormatFieldsFilter(KeyValuePair<string, string> p)
        {
            if (!p.Value.StartsWith("new"))
                return new KeyValuePair<string, string>(CodeSnippet.Translate(p.Key, dict), p.Value);
        }*/

        public string MediaSubTypeToString(Guid guid)
        {
            foreach (FieldInfo m in typeof (MediaSubType).GetFields())
                if ((Guid) m.GetValue(null) == guid)
                    return "MEDIASUBTYPE_" + m.Name.ToUpperInvariant();
            return GuidName(Graph.GuidToString(guid));
        }

        public override string MediaTypeToString(Guid guid)
        {
            foreach (FieldInfo m in typeof (MediaType).GetFields())
                if ((Guid) m.GetValue(null) == guid)
                    return "MEDIATYPE_" + m.Name;
            return GuidName(Graph.GuidToString(guid));
        }

        public override string GenCode(bool useDirectConnect, Graph graph)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//Don't forget to change project settings:");
            sb.AppendLine("//1. C++: add include path to DirectShow include folder (such as c:\\dxsdk\\include)");
            sb.AppendLine("//2. Link: add link path to DirectShow lib folder (such as c:\\dxsdk\\lib).");
            sb.AppendLine("//3. Link: add strmiids.lib and quartz.lib");
            sb.AppendLine();
            sb.AppendLine("#include \"stdafx.h\"");
            sb.AppendLine("#include <DShow.h>");
            sb.AppendLine("#include <atlbase.h>");
            sb.AppendLine("#include <initguid.h>");
            sb.AppendLine("#include <dvdmedia.h>");
            sb.AppendLine();
            sb.AppendLine(checkTpl.Generate());

            if (useDirectConnect) needGetPin = true;
            directConnect = useDirectConnect;

            StringBuilder sb_def = new StringBuilder();
            Dictionary<string, bool> defined = new Dictionary<string, bool>(); //guid => define_guid text
            foreach (HistoryItem hi in history.Items)
            {
                string def = hi.Define(this);
                if (!defined.ContainsKey(def)) //do not repeat
                {
                    sb_def.Append(def);
                    defined.Add(def, true);
                }
                if (def.Length > 0) sb_def.AppendLine();
            }
            if (needCreateFilterProc)
            {
                sb.AppendLine("CComPtr<IBaseFilter> CreateFilter(WCHAR* displayName)");
                sb.AppendLine("{");
                sb.AppendLine("    CComPtr<IBindCtx> pBindCtx;");
                sb.AppendLine("    HRESULT hr = CreateBindCtx(0, &pBindCtx);");
                sb.AppendLine("    if (hrcheck(hr, \"Can't create bind context\"))");
                sb.AppendLine("        return NULL;");
                sb.AppendLine();
                sb.AppendLine("    ULONG chEaten = 0;");
                sb.AppendLine("    CComPtr<IMoniker> pMoniker;");
                sb.AppendLine("    hr = MkParseDisplayName(pBindCtx, displayName, &chEaten, &pMoniker);");
                sb.AppendLine("    if (hrcheck(hr, \"Can't create parse display name of the filter\"))");
                sb.AppendLine("        return NULL;");
                sb.AppendLine();
                sb.AppendLine("    CComPtr<IBaseFilter> pFilter;");
                sb.AppendLine("    if (SUCCEEDED(hr))");
                sb.AppendLine("    {");
                sb.AppendLine("        hr = pMoniker->BindToObject(pBindCtx, NULL, IID_IBaseFilter, (void**)&pFilter);");
                sb.AppendLine("        if (hrcheck(hr, \"Can't bind moniker to filter object\"))");
                sb.AppendLine("            return NULL;");
                sb.AppendLine("    }");
                sb.AppendLine("    return pFilter;");
                sb.AppendLine("}");
                sb.AppendLine();
            }
            if (needGetPin)
            {
                sb.AppendLine("CComPtr<IPin> GetPin(IBaseFilter *pFilter, LPCOLESTR pinname)");
                sb.AppendLine("{");
                sb.AppendLine("    CComPtr<IEnumPins>  pEnum;");
                sb.AppendLine("    CComPtr<IPin>       pPin;");
                sb.AppendLine();
                sb.AppendLine("    HRESULT hr = pFilter->EnumPins(&pEnum);");
                sb.AppendLine("    if (hrcheck(hr, \"Can't enumerate pins.\"))");
                sb.AppendLine("        return NULL;");
                sb.AppendLine();
                sb.AppendLine("    while(pEnum->Next(1, &pPin, 0) == S_OK)");
                sb.AppendLine("    {");
                sb.AppendLine("        PIN_INFO pinfo;");
                sb.AppendLine("        pPin->QueryPinInfo(&pinfo);");
                sb.AppendLine("        BOOL found = !wcsicmp(pinname, pinfo.achName);");
                sb.AppendLine("        if (pinfo.pFilter) pinfo.pFilter->Release();");
                sb.AppendLine("        if (found)");
                sb.AppendLine("            return pPin;");
                sb.AppendLine("        pPin.Release();");
                sb.AppendLine("    }");
                sb.AppendLine("    printf(\"Pin not found!\\n\");");
                sb.AppendLine("    return NULL;  ");
                sb.AppendLine("}");
                sb.AppendLine();
            }

            sb.AppendLine(sb_def.ToString());

            StringBuilder sb_bld = new StringBuilder();
            foreach (HistoryItem hi in history.Items)
                sb_bld.AppendLine(hi.Build(this, graph));

            sb.AppendLine();
            sb.Append("HRESULT BuildGraph(IGraphBuilder *pGraph");
            for(int i = 0; i < srcFileNames.Count; i++)
                sb.Append(", LPCOLESTR srcFile" + (i + 1).ToString());
            for (int i = 0; i < dstFileNames.Count; i++)
                sb.Append(", LPCOLESTR dstFile" + (i + 1).ToString());
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLine("    HRESULT hr = S_OK;");
            sb.AppendLine();
            sb.AppendLine("    //graph builder");
            sb.AppendLine("    CComPtr<ICaptureGraphBuilder2> pBuilder;");
            sb.AppendLine("    hr = pBuilder.CoCreateInstance(CLSID_CaptureGraphBuilder2);");
            sb.AppendLine("    CHECK_HR(hr, \"Can't create Capture Graph Builder\");");
            sb.AppendLine("    hr = pBuilder->SetFiltergraph(pGraph);");
            sb.AppendLine("    CHECK_HR(hr, \"Can't SetFiltergraph\");");
            sb.AppendLine();
            sb.Append(sb_bld.ToString());
            sb.AppendLine("    return S_OK;");
            sb.Append("}");


            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("//int _tmain(int argc, _TCHAR* argv[]) //use this line in VS2008");
            sb.AppendLine("int main(int argc, char* argv[])");
            sb.AppendLine("{");
            sb.AppendLine("    CoInitialize(NULL);");
            sb.AppendLine("    CComPtr<IGraphBuilder> graph;");
            sb.AppendLine("    graph.CoCreateInstance(CLSID_FilterGraph);");
            sb.AppendLine();
            sb.AppendLine("    printf(\"Building graph...\\n\");");
            sb.Append("    HRESULT hr = BuildGraph(graph");
            foreach (string fn in srcFileNames)
                sb.Append(", L\"" + fn.Replace("\\","\\\\") + "\"");
            foreach (string fn in dstFileNames)
                sb.Append(", L\"" + fn.Replace("\\", "\\\\") + "\"");
            sb.AppendLine(");");
            sb.AppendLine("    if (hr==S_OK) {");
            sb.AppendLine("        printf(\"Running\");");
            sb.AppendLine("        CComQIPtr<IMediaControl, &IID_IMediaControl> mediaControl(graph);");
            sb.AppendLine("        hr = mediaControl->Run();");
            sb.AppendLine("        CHECK_HR(hr, \"Can't run the graph\");");
            sb.AppendLine("        CComQIPtr<IMediaEvent, &IID_IMediaEvent> mediaEvent(graph);");
            sb.AppendLine("        BOOL stop = FALSE;");
            sb.AppendLine("        while(!stop) ");
            sb.AppendLine("        {");
            sb.AppendLine("            long ev=0, p1=0, p2=0;");
            sb.AppendLine("            Sleep(500);");
            sb.AppendLine("            printf(\".\");");
            sb.AppendLine("            if (mediaEvent->GetEvent(&ev, &p1, &p2, 0)==S_OK)");
            sb.AppendLine("            {");
            sb.AppendLine("                if (ev == EC_COMPLETE || ev == EC_USERABORT)");
            sb.AppendLine("                {");
            sb.AppendLine("                    printf(\"Done!\\n\");");
            sb.AppendLine("                    stop = TRUE;");
            sb.AppendLine("                }");
            sb.AppendLine("                else");
            sb.AppendLine("                if (ev == EC_ERRORABORT)");
            sb.AppendLine("                {");
            sb.AppendLine("                    printf(\"An error occured: HRESULT=%x\\n\", p1);");
            sb.AppendLine("                    mediaControl->Stop();");
            sb.AppendLine("                    stop = TRUE;");
            sb.AppendLine("                }");
            sb.AppendLine("                mediaEvent->FreeEventParams(ev, p1, p2);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("    CoUninitialize();");
            sb.AppendLine("    return 0;");
            sb.AppendLine("}");

            return sb.ToString();
        }

        

        void InitGuidsTable()
        {
            known.Add("{CC58E280-8AA1-11D1-B3F1-00AA003761C5}", "CLSID_SmartTee");
            known.Add("{BF87B6E0-8C27-11D0-B3F0-00AA003761C5}", "CLSID_CaptureGraphBuilder");
            known.Add("{BF87B6E1-8C27-11D0-B3F0-00AA003761C5}", "CLSID_CaptureGraphBuilder2");
            known.Add("{E436EBB0-524F-11CE-9F53-0020AF0BA770}", "CLSID_ProtoFilterGraph");
            known.Add("{E436EBB1-524F-11CE-9F53-0020AF0BA770}", "CLSID_SystemClock");
            known.Add("{E436EBB2-524F-11CE-9F53-0020AF0BA770}", "CLSID_FilterMapper");
            known.Add("{E436EBB3-524F-11CE-9F53-0020AF0BA770}", "CLSID_FilterGraph");
            known.Add("{E436EBB8-524F-11CE-9F53-0020AF0BA770}", "CLSID_FilterGraphNoThread");
            known.Add("{E4BBD160-4269-11CE-838D-00AA0055595A}", "CLSID_MPEG1Doc");
            known.Add("{701722E0-8AE3-11CE-A85C-00AA002FEAB5}", "CLSID_FileSource");
            known.Add("{26C25940-4CA9-11CE-A828-00AA002FEAB5}", "CLSID_MPEG1PacketPlayer");
            known.Add("{336475D0-942A-11CE-A870-00AA002FEAB5}", "CLSID_MPEG1Splitter");
            known.Add("{FEB50740-7BEF-11CE-9BD9-0000E202599C}", "CLSID_CMpegVideoCodec");
            known.Add("{4A2286E0-7BEF-11CE-9BD9-0000E202599C}", "CLSID_CMpegAudioCodec");
            known.Add("{E30629D3-27E5-11CE-875D-00608CB78066}", "CLSID_TextRender");
            known.Add("{F8388A40-D5BB-11D0-BE5A-0080C706568E}", "CLSID_InfTee");
            known.Add("{1B544C20-FD0B-11CE-8C63-00AA0044B51E}", "CLSID_AviSplitter");
            known.Add("{1B544C21-FD0B-11CE-8C63-00AA0044B51E}", "CLSID_AviReader");
            known.Add("{1B544C22-FD0B-11CE-8C63-00AA0044B51E}", "CLSID_VfwCapture");
            known.Add("{E436EBB4-524F-11CE-9F53-0020AF0BA770}", "CLSID_FGControl");
            known.Add("{44584800-F8EE-11CE-B2D4-00DD01101B85}", "CLSID_MOVReader");
            known.Add("{D51BD5A0-7548-11CF-A520-0080C77EF58A}", "CLSID_QuickTimeParser");
            known.Add("{FDFE9681-74A3-11D0-AFA7-00AA00B67A42}", "CLSID_QTDec");
            known.Add("{D3588AB0-0781-11CE-B03A-0020AF0BA770}", "CLSID_AVIDoc");
            known.Add("{70E102B0-5556-11CE-97C0-00AA0055595A}", "CLSID_VideoRenderer");
            known.Add("{1643E180-90F5-11CE-97D5-00AA0055595A}", "CLSID_Colour");
            known.Add("{1DA08500-9EDC-11CF-BC10-00AA00AC74F6}", "CLSID_Dither");
            known.Add("{07167665-5011-11CF-BF33-00AA0055595A}", "CLSID_ModexRenderer");
            known.Add("{E30629D1-27E5-11CE-875D-00608CB78066}", "CLSID_AudioRender");
            known.Add("{05589FAF-C356-11CE-BF01-00AA0055595A}", "CLSID_AudioProperties");
            known.Add("{79376820-07D0-11CF-A24D-0020AFD79767}", "CLSID_DSoundRender");
            known.Add("{E30629D2-27E5-11CE-875D-00608CB78066}", "CLSID_AudioRecord");
            known.Add("{2CA8CA52-3C3F-11D2-B73D-00C04FB6BD3D}", "CLSID_AudioInputMixerProperties");
            known.Add("{CF49D4E0-1115-11CE-B03A-0020AF0BA770}", "CLSID_AVIDec");
            known.Add("{A888DF60-1E90-11CF-AC98-00AA004C0FA9}", "CLSID_AVIDraw");
            known.Add("{6A08CF80-0E18-11CF-A24D-0020AFD79767}", "CLSID_ACMWrapper");
            known.Add("{E436EBB5-524F-11CE-9F53-0020AF0BA770}", "CLSID_AsyncReader");
            known.Add("{E436EBB6-524F-11CE-9F53-0020AF0BA770}", "CLSID_URLReader");
            known.Add("{E436EBB7-524F-11CE-9F53-0020AF0BA770}", "CLSID_PersistMonikerPID");
            known.Add("{5F2759C0-7685-11CF-8B23-00805F6CEF60}", "CLSID_AMovie");
            known.Add("{D76E2820-1563-11CF-AC98-00AA004C0FA9}", "CLSID_AVICo");
            known.Add("{8596E5F0-0DA5-11D0-BD21-00A0C911CE86}", "CLSID_FileWriter");
            known.Add("{E2510970-F137-11CE-8B67-00AA00A3F1A6}", "CLSID_AviDest");
            known.Add("{C647B5C0-157C-11D0-BD23-00A0C911CE86}", "CLSID_AviMuxProptyPage");
            known.Add("{0A9AE910-85C0-11D0-BD42-00A0C911CE86}", "CLSID_AviMuxProptyPage1");
            known.Add("{07B65360-C445-11CE-AFDE-00AA006C14F4}", "CLSID_AVIMIDIRender");
            known.Add("{187463A0-5BB7-11D3-ACBE-0080C75E246E}", "CLSID_WMAsfReader");
            known.Add("{7C23220E-55BB-11D3-8B16-00C04FB6BD3D}", "CLSID_WMAsfWriter");
            known.Add("{AFB6C280-2C41-11D3-8A60-0000F81E0E4A}", "CLSID_MPEG2Demultiplexer");
            known.Add("{3AE86B20-7BE8-11D1-ABE6-00A0C905F375}", "CLSID_MMSPLITTER");
            known.Add("{B1B77C00-C3E4-11CF-AF79-00AA00B67A42}", "CLSID_DVVideoCodec");
            known.Add("{13AA3650-BB6F-11D0-AFB9-00AA00B67A42}", "CLSID_DVVideoEnc");
            known.Add("{4EB31670-9FC6-11CF-AF6E-00AA00B67A42}", "CLSID_DVSplitter");
            known.Add("{129D7E40-C10D-11D0-AFB9-00AA00B67A42}", "CLSID_DVMux");
            known.Add("{060AF76C-68DD-11D0-8FC1-00C04FD9189D}", "CLSID_SeekingPassThru");
            known.Add("{6E8D4A20-310C-11D0-B79A-00AA003767A7}", "CLSID_Line21Decoder");
            known.Add("{E4206432-01A1-4BEE-B3E1-3702C8EDC574}", "CLSID_Line21Decoder2");
            known.Add("{CD8743A1-3736-11D0-9E69-00C04FD7C15B}", "CLSID_OverlayMixer");
            known.Add("{814B9800-1C88-11D1-BAD9-00609744111A}", "CLSID_VBISurfaces");
            known.Add("{70BC06E0-5666-11D3-A184-00105AEF9F33}", "CLSID_WSTDecoder");
            known.Add("{301056D0-6DFF-11D2-9EEB-006008039E37}", "CLSID_MjpegDec");
            known.Add("{B80AB0A0-7416-11D2-9EEB-006008039E37}", "CLSID_MJPGEnc");
        }

    }

    class CodeGenCS : CodeGenBase
    {
        CodeSnippet addFiltMonTpl, defineDsTpl, addFiltDsKnownTpl, addFiltDsUnknownTpl, checkTpl;
        

        public override string ToString()
        {
            return "C#";
        }

        public CodeGenCS()
        {
            InitGuidsTable();

            defineDsTpl = new CodeSnippet("Define CLSID for a custom DirectShow filter", "defineDsTpl",
                "            Guid $clsname = new Guid(\"$guid\"); //$file\r\n",
                "$clsname - name of variable to hold Guid\r\n" +
                "$guid - GUID digits\r\n" +
                "$file - name of file containing the filter\r\n");
            defineDsTpl.SetVars(new string[] {
                "$clsname", "CLSID_DivXDecoderFilter",
                "$guid", "{78766964-0000-0010-8000-00AA00389B71}",
                "$file", "divxdec.ax"
            });

            addFiltDsKnownTpl = new CodeSnippet("Create a standard DirectShow filter", "addFiltDsKnownTpl",
                "            //add $name\r\n" +
                "            IBaseFilter $var = (IBaseFilter) new $clsname();\r\n",
                "$name - name of the filter\r\n" +
                "$var - variable to hold IBaseFilter\r\n" +
                "$clsname - name of DirectShowLib class for this filter");
            addFiltDsKnownTpl.SetVars(new string[] {
                "$name", "DV Splitter",
                "$var", "pDVSplitter",
                "$clsname", "DVSplitter"
            });

            addFiltDsUnknownTpl = new CodeSnippet("Create a custom DirectShow filter", "addFiltDsUnknownTpl",
                            "            //add $name\r\n" +
                            "            IBaseFilter $var = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID($clsname));\r\n",
                            "$name - name of the filter\r\n" +
                            "$var - variable to hold IBaseFilter\r\n" +
                            "$clsname - name of Guid variable with GUID for this filter");
            addFiltDsUnknownTpl.SetVars(new string[] {
                "$name", "DivX Decoder Filter",
                "$var", "pDivXDecoderFilter",
                "$clsname", "CLSID_DivXDecoderFilter"
            });

            insertTpl = new CodeSnippet("Insert filter to graph", "insertTpl",
                "            hr = pGraph.AddFilter($var, \"$name\");\r\n" +
                "            checkHR(hr, \"Can't add $name to graph\");\r\n",
                "$var - variable holding IBaseFilter\r\n" +
                "$name - name of the filter");
            insertTpl.SetVars(new string[] {
                "$name", "DivX Decoder Filter",
                "$var", "pDivXDecoderFilter"                
            });

            setSrcFileTpl = new CodeSnippet("Set source file to IFileSourceFilter", "setSrcFileTpl",
                "            //set source filename\r\n" +
                "            IFileSourceFilter $srcvar = $var as IFileSourceFilter;\r\n" +
                "            if ($srcvar == null)\r\n" +
                "                checkHR(unchecked((int)0x80004002), \"Can't get IFileSourceFilter\");\r\n" +
                "            hr = $srcvar.Load($filename, null);\r\n" +
                "            checkHR(hr, \"Can't load file\");\r\n",
                "$srcvar - variable of type IFileSourceFilter\r\n" +
                "$var - variable holding IBaseFilter\r\n" +
                "$filename - variable holding name of file to open");
            setSrcFileTpl.SetVars(new string[] {
                "$srcvar", "pFileSourceAsync_src",
                "$var", "pFileSourceAsync",
                "$filename", "srcFile1"
            });

            setDstFileTpl = new CodeSnippet("Set destination file to IFileSinkFilter", "setDstFileTpl",
                "            //set destination filename\r\n" +
                "            IFileSinkFilter $dstvar = $var as IFileSinkFilter;\r\n" +
                "            if ($dstvar == null)\r\n" +
                "                checkHR(unchecked((int)0x80004002), \"Can't get IFileSinkFilter\");\r\n" +
                "            hr = $dstvar.SetFileName($filename, null);\r\n" +
                "            checkHR(hr, \"Can't set filename\");\r\n",
                "$dstvar - variable of type IFileSinkFilter\r\n" +
                "$var - variable holding IBaseFilter\r\n" +
                "$filename - variable holding name of file to open");
            setDstFileTpl.SetVars(new string[] {
                "$dstvar", "pFilewriter_sink",
                "$var", "pFilewriter",
                "$filename", "dstFile1"
            });

            addFiltMonTpl = new CodeSnippet("Create a filter by display name", "addFiltMonTpl",
                "            //add $name\r\n" +
                "            IBaseFilter $var = CreateFilter(@\"$displayname\");\r\n",
                "$name - name of the filter\r\n" +
                "$var - variable holding IBaseFilter\r\n" +
                "$displayname - display name of the filter\r\n");
            addFiltMonTpl.SetVars(new string[] {
                "$name", "Fraps Video Decompressor",
                "$var", "pFrapsVideoDecompressor",
                "$displayname", @"@device:cm:{33D9A760-90C8-11D0-BD43-00A0C911CE86}\fps1"
            });

            connectTpl = new CodeSnippet("Connect two filters", "connectTpl",
                "            //connect $pair\r\n" +
                "            hr = pBuilder.RenderStream(null, $majortype, $var1, null, $var2);\r\n" +
                "            checkHR(hr, \"Can't connect $pair\");\r\n\r\n",
                "$pair - names of connecting filters\r\n" +
                "$majortype - major media type of connection\r\n" +
                "$var1, $var2 - variables holding IBaseFilter of connecting filters");
            connectTpl.SetVars(new string[] {
                "$pair", "File Source (Async.) and AVI Splitter",
                "$majortype", "MediaType.Stream",
                "$var1", "pFileSourceAsync",
                "$var2", "pAVISplitter"
            });

            connectDirectTpl = new CodeSnippet("Connect two filters directly", "connectDirectTpl",
                "            //connect $pair\r\n" +
                "            hr = pGraph.ConnectDirect(GetPin($var1, \"$pin1\"), GetPin($var2, \"$pin2\"), null);\r\n" +
                "            checkHR(hr, \"Can't connect $pair\");\r\n\r\n",
                "$pair - names of connecting filters\r\n" +
                "$var1, $var2 - variables holding IBaseFilter of connecting filters\r\n" +
                "$pin1, $pin2 - names of connecting pins");
            connectDirectTpl.SetVars(new string[] {
                "$pair", "File Source (Async.) and AVI Splitter",
                "$majortype", "MediaType.Stream",
                "$var1", "pFileSourceAsync",
                "$var2", "pAVISplitter",
                "$pin1", "Output",
                "$pin2", "input pin"
            });

            checkTpl = new CodeSnippet("Check HRESULT for errors", "checkTpl",
            "        static void checkHR(int hr, string msg)\r\n" +
            "        {\r\n" +
            "            if (hr < 0)\r\n" +
            "            {\r\n" +
            "                Console.WriteLine(msg);\r\n" +
            "                DsError.ThrowExceptionForHR(hr);\r\n" +
            "            }\r\n" +
            "        }\r\n", "");

            snippets = new CodeSnippet[] { 
                defineDsTpl, addFiltDsKnownTpl, addFiltDsUnknownTpl, addFiltMonTpl, setSrcFileTpl, setDstFileTpl,
                insertTpl, connectTpl, connectDirectTpl, checkTpl
            };

            LoadTemplates();
        }

        public override string DefineAddFilterDS(HIAddFilterDS hi)
        {
            string s;
            if (known.TryGetValue(hi.CLSID, out s))
            {
                hi.clsname = s;
                return ""; //no need to define
            }
            return defineDsTpl.GenerateWith(new string[] {
                "$clsname", hi.clsname, "$guid", hi.CLSID, "$file", hi.FileName
            });
        }

        public override string DefineSetFormat(HISetFormat hi)
        {
            return "";
        }

        public override string DefineConnect(HIConnect hi)
        {
            return "";
        }

        public override string BuildAddFilterDS(HIAddFilterDS hi, Graph graph)
        {
            string create, s;
            if (known.TryGetValue(hi.CLSID, out s))
            {
                create = addFiltDsKnownTpl.GenerateWith(new string[] {
                    "$name", hi.Name, "$var", hi.var, "$clsname", s
                });
            }
            else
            {
                create = addFiltDsUnknownTpl.GenerateWith(new string[] {
                    "$name", hi.Name, "$var", hi.var, "$clsname", hi.clsname
                });
            }
            return create + Insert(hi, graph);
        }

        public override string BuildAddFilterMon(HIAddFilterMon hi, Graph graph)
        {
            return addFiltMonTpl.GenerateWith(new string[] {
                "$name", hi.Name, "$var", hi.var, "$displayname", hi.DisplayName
            }) + Insert(hi, graph);
        }


        void CreateMediaType(string var, AMMediaType mt, StringBuilder sb)
        {
            sb.AppendFormat("            AMMediaType {0} = new AMMediaType();\r\n", var);
            sb.AppendFormat("            {0}.majorType = {1};\r\n", var, MediaTypeToString(mt.majorType));
            sb.AppendFormat("            {0}.subType = {1};\r\n", var, MediaSubTypeToString(mt.subType));
            sb.AppendFormat("            {0}.formatType = FormatType.{1};\r\n", var, DsToString.MediaFormatTypeToString(mt.formatType));
            sb.AppendFormat("            {0}.fixedSizeSamples = {1};\r\n", var, mt.fixedSizeSamples.ToString().ToLowerInvariant());
            sb.AppendFormat("            {0}.formatSize = {1};\r\n", var, mt.formatSize);
            sb.AppendFormat("            {0}.sampleSize = {1};\r\n", var, mt.sampleSize);
            sb.AppendFormat("            {0}.temporalCompression = {1};\r\n", var, mt.temporalCompression.ToString().ToLowerInvariant());
            string fmtvar = var.Replace("pmt", "format");
            MediaTypeProps mtp = MediaTypeProps.CreateMTProps(mt);
            sb.AppendFormat("            {1} {0} = new {1}();\r\n", fmtvar, mtp.FormatClass());
            mtp.IterFormatFields(true, delegate(string fldname, string fldvalue)
                { sb.AppendFormat("            {0}.{1} = {2};\r\n", fmtvar, fldname, fldvalue); });
            sb.AppendFormat("            {0}.formatPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf({1}));\r\n", var, fmtvar);
            sb.AppendFormat("            Marshal.StructureToPtr({0}, {1}.formatPtr, false);\r\n", fmtvar, var);
        }

        public override string SetFormat(HISetFormat hi)
        {
            HistoryItem fhi = history.FindByRealName(hi.filter);
            string fvar = (fhi != null) ? fhi.var : "?";
            StringBuilder sb = new StringBuilder();
            CreateMediaType(hi.var, hi.mt, sb);            
            sb.AppendFormat("            hr = ((IAMStreamConfig)GetPin({0}, \"{1}\")).SetFormat({2});\r\n", fvar, hi.pin, hi.var);
            sb.AppendFormat("            DsUtils.FreeAMMediaType({0});\r\n", hi.var);
            sb.Append("            checkHR(hr, \"Can't set format\");\r\n");
            sb.AppendLine();
            return sb.ToString();
        }


        protected override string SetSampleGrabberMediaType(AMMediaType mt, HistoryItem hi)
        {
            string mtvar = hi.var + "_pmt";
            StringBuilder sb = new StringBuilder();
            CreateMediaType(mtvar, mt, sb);
            sb.AppendFormat("            hr = ((ISampleGrabber){0}).SetMediaType({1});\r\n", hi.var, mtvar);
            sb.AppendFormat("            DsUtils.FreeAMMediaType({0});\r\n", mtvar);
            sb.Append("            checkHR(hr, \"Can't set media type to sample grabber\");\r\n");
            sb.AppendLine();
            return sb.ToString();
        }

        public static string MediaSubTypeToString(Guid guid)
        {
            foreach (FieldInfo m in typeof (MediaSubType).GetFields())
                if ((Guid) m.GetValue(null) == guid)
                    return "MediaSubType." + m.Name;
            return "new Guid(\"" + Graph.GuidToString(guid) + "\")";
        }

        public override string MediaTypeToString(Guid guid)
        {
            foreach (FieldInfo m in typeof (MediaType).GetFields())
                if ((Guid) m.GetValue(null) == guid)
                    return "MediaType." + m.Name;
            return "new Guid(\"" + Graph.GuidToString(guid) + "\")";
        }

        public override string GenCode(bool useDirectConnect, Graph graph)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//Don't forget to add reference to DirectShowLib in your project.");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.InteropServices.ComTypes;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendLine("using DirectShowLib;");
            sb.AppendLine();
            sb.AppendLine("namespace graphcode");
            sb.AppendLine("{");
            sb.AppendLine("    class Program");
            sb.AppendLine("    {");
            sb.AppendLine(checkTpl.Generate());

            if (useDirectConnect) needGetPin = true;
            directConnect = useDirectConnect;
            //define 
            StringBuilder sb_def = new StringBuilder();
            Dictionary<string, bool> defined = new Dictionary<string, bool>();
            foreach (HistoryItem hi in history.Items)
            {
                string def = hi.Define(this);
                if (!defined.ContainsKey(def)) //do not repeat
                {
                    sb_def.Append(def);
                    defined.Add(def, true);
                }
            }

            StringBuilder sb_bld = new StringBuilder();
            foreach (HistoryItem hi in history.Items)
                sb_bld.Append(hi.Build(this, graph));

            sb.Append("        static void BuildGraph(IGraphBuilder pGraph");
            for (int i = 0; i < srcFileNames.Count; i++)
                sb.Append(", string srcFile" + (i + 1).ToString());
            for (int i = 0; i < dstFileNames.Count; i++)
                sb.Append(", string dstFile" + (i + 1).ToString());
            sb.AppendLine(")");
            sb.AppendLine("        {");
            sb.AppendLine("            int hr = 0;");
            sb.AppendLine();
            sb.AppendLine("            //graph builder");
            sb.AppendLine("            ICaptureGraphBuilder2 pBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();");
            sb.AppendLine("            hr = pBuilder.SetFiltergraph(pGraph);");
            sb.AppendLine("            checkHR(hr, \"Can't SetFiltergraph\");");
            sb.AppendLine();
            sb.Append(sb_def);
            sb.AppendLine();
            sb.Append(sb_bld);
            sb.AppendLine("        }"); //end of buildgraph
            sb.AppendLine();
            sb.AppendLine("        static void Main(string[] args)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                IGraphBuilder graph = (IGraphBuilder)new FilterGraph();");
            sb.AppendLine("                Console.WriteLine(\"Building graph...\");");
            sb.Append("                BuildGraph(graph");
            foreach (string fn in srcFileNames)
                sb.Append(", @\"" + fn + "\"");
            foreach (string fn in dstFileNames)
                sb.Append(", @\"" + fn + "\"");
            sb.AppendLine(");");
            sb.AppendLine("                Console.WriteLine(\"Running...\");");
            sb.AppendLine("                IMediaControl mediaControl = (IMediaControl)graph;");
            sb.AppendLine("                IMediaEvent mediaEvent = (IMediaEvent)graph;");
            sb.AppendLine("                int hr = mediaControl.Run();");
            sb.AppendLine("                checkHR(hr, \"Can't run the graph\");");
            sb.AppendLine("                bool stop = false;");
            sb.AppendLine("                int n = 0;");
            sb.AppendLine("                while (!stop)");
            sb.AppendLine("                {");
            sb.AppendLine("                    System.Threading.Thread.Sleep(500);");
            sb.AppendLine("                    Console.Write(\".\");");
            sb.AppendLine("                    EventCode ev;");
            sb.AppendLine("                    IntPtr p1, p2;");
            sb.AppendLine("                    if (mediaEvent.GetEvent(out ev, out p1, out p2, 0) == 0)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        if (ev == EventCode.Complete || ev == EventCode.UserAbort)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            Console.WriteLine(\"Done!\");");
            sb.AppendLine("                            stop = true;");
            sb.AppendLine("                        }");
            sb.AppendLine("                        else");
            sb.AppendLine("                        if (ev == EventCode.ErrorAbort)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            Console.WriteLine(\"An error occured: HRESULT={0:X}\", p1);");
            sb.AppendLine("                            mediaControl.Stop();");
            sb.AppendLine("                            stop = true;");
            sb.AppendLine("                        }");
            sb.AppendLine("                        mediaEvent.FreeEventParams(ev, p1, p2);");
            sb.AppendLine("                    }");
            sb.AppendLine("                    // stop after 10 seconds");
            sb.AppendLine("                    n++;");
            sb.AppendLine("                    if (n > 20)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        Console.WriteLine(\"stopping..\");");
            sb.AppendLine("                        mediaControl.Stop();");
            sb.AppendLine("                        stop = true;");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (COMException ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine(\"COM error: \" + ex.ToString());");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine(\"Error: \" + ex.ToString());");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            
            if (needCreateFilterProc)
            {
                sb.AppendLine();
                sb.AppendLine("        public static IBaseFilter CreateFilter(string displayName)");
                sb.AppendLine("        {");
                sb.AppendLine("            int hr = 0;");
                sb.AppendLine("            IBaseFilter filter = null;");
                sb.AppendLine("            IBindCtx bindCtx = null;");
                sb.AppendLine("            IMoniker moniker = null;");
                sb.AppendLine("");
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                hr = CreateBindCtx(0, out bindCtx);");
                sb.AppendLine("                Marshal.ThrowExceptionForHR(hr);");
                sb.AppendLine("");
                sb.AppendLine("                int eaten;");
                sb.AppendLine("                hr = MkParseDisplayName(bindCtx, displayName, out eaten, out moniker);");
                sb.AppendLine("                Marshal.ThrowExceptionForHR(hr);");
                sb.AppendLine("");
                sb.AppendLine("                Guid guid = typeof(IBaseFilter).GUID;");
                sb.AppendLine("                object obj;");
                sb.AppendLine("                moniker.BindToObject(bindCtx, null, ref guid, out obj);");
                sb.AppendLine("                filter = (IBaseFilter)obj;");
                sb.AppendLine("            }");
                //sb.AppendLine("            catch");
                //sb.AppendLine("            {");
                //sb.AppendLine("                throw;");
                //sb.AppendLine("            }");
                sb.AppendLine("            finally");
                sb.AppendLine("            {");
                sb.AppendLine("                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);");
                sb.AppendLine("                if (moniker != null) Marshal.ReleaseComObject(moniker);");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return filter;");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        [DllImport(\"ole32.dll\")]");
                sb.AppendLine("        public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);");
                sb.AppendLine("");
                sb.AppendLine("        [DllImport(\"ole32.dll\")]");
                sb.AppendLine("        public static extern int MkParseDisplayName(IBindCtx pcb, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out int pchEaten, out IMoniker ppmk);");
                sb.AppendLine();
            }

            if (needGetPin)
            {
                sb.AppendLine("        static IPin GetPin(IBaseFilter filter, string pinname)");
                sb.AppendLine("        {");
                sb.AppendLine("            IEnumPins epins;");
                sb.AppendLine("            int hr = filter.EnumPins(out epins);");
                sb.AppendLine("            checkHR(hr, \"Can't enumerate pins\");");
                sb.AppendLine("            IntPtr fetched = Marshal.AllocCoTaskMem(4);");
                sb.AppendLine("            IPin[] pins = new IPin[1];");
                sb.AppendLine("            while (epins.Next(1, pins, fetched) == 0)");
                sb.AppendLine("            {");
                sb.AppendLine("                PinInfo pinfo;");
                sb.AppendLine("                pins[0].QueryPinInfo(out pinfo);");
                sb.AppendLine("                bool found = (pinfo.name == pinname);");
                sb.AppendLine("                DsUtils.FreePinInfo(pinfo);");
                sb.AppendLine("                if (found)");
                sb.AppendLine("                    return pins[0];");
                sb.AppendLine("            }");
                sb.AppendLine("            checkHR(-1, \"Pin not found\");");
                sb.AppendLine("            return null;");
                sb.AppendLine("        }");
                sb.AppendLine();
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("} ");

            return sb.ToString();
        }

        void InitGuidsTable()
        {
            known.Add("{CC58E280-8AA1-11D1-B3F1-00AA003761C5}", "SmartTee"); 
            known.Add("{BF87B6E0-8C27-11D0-B3F0-00AA003761C5}", "CaptureGraphBuilder");
            known.Add("{BF87B6E1-8C27-11D0-B3F0-00AA003761C5}", "CaptureGraphBuilder2");
            known.Add("{E436EBB0-524F-11CE-9F53-0020AF0BA770}", "ProtoFilterGraph");
            known.Add("{E436EBB1-524F-11CE-9F53-0020AF0BA770}", "SystemClock");
            known.Add("{E436EBB2-524F-11CE-9F53-0020AF0BA770}", "FilterMapper"); 
            known.Add("{E436EBB3-524F-11CE-9F53-0020AF0BA770}", "FilterGraph");
            known.Add("{E436EBB8-524F-11CE-9F53-0020AF0BA770}", "FilterGraphNoThread");
            known.Add("{E4BBD160-4269-11CE-838D-00AA0055595A}", "MPEG1Doc");
            known.Add("{701722E0-8AE3-11CE-A85C-00AA002FEAB5}", "FileSource");
            known.Add("{26C25940-4CA9-11CE-A828-00AA002FEAB5}", "MPEG1PacketPlayer");
            known.Add("{336475D0-942A-11CE-A870-00AA002FEAB5}", "MPEG1Splitter");
            known.Add("{FEB50740-7BEF-11CE-9BD9-0000E202599C}", "CMpegVideoCodec");
            known.Add("{4A2286E0-7BEF-11CE-9BD9-0000E202599C}", "CMpegAudioCodec");
            known.Add("{E30629D3-27E5-11CE-875D-00608CB78066}", "TextRender");
            known.Add("{F8388A40-D5BB-11D0-BE5A-0080C706568E}", "InfTee");
            known.Add("{1B544C20-FD0B-11CE-8C63-00AA0044B51E}", "AviSplitter");
            known.Add("{1B544C21-FD0B-11CE-8C63-00AA0044B51E}", "AviReader");
            known.Add("{1B544C22-FD0B-11CE-8C63-00AA0044B51E}", "VfwCapture");
            known.Add("{E436EBB4-524F-11CE-9F53-0020AF0BA770}", "FGControl");
            known.Add("{44584800-F8EE-11CE-B2D4-00DD01101B85}", "MOVReader");
            known.Add("{D51BD5A0-7548-11CF-A520-0080C77EF58A}", "QuickTimeParser");
            known.Add("{FDFE9681-74A3-11D0-AFA7-00AA00B67A42}", "QTDec");
            known.Add("{D3588AB0-0781-11CE-B03A-0020AF0BA770}", "AVIDoc");
            known.Add("{70E102B0-5556-11CE-97C0-00AA0055595A}", "VideoRenderer");
            known.Add("{1643E180-90F5-11CE-97D5-00AA0055595A}", "Colour");
            known.Add("{1DA08500-9EDC-11CF-BC10-00AA00AC74F6}", "Dither");
            known.Add("{07167665-5011-11CF-BF33-00AA0055595A}", "ModexRenderer");
            known.Add("{E30629D1-27E5-11CE-875D-00608CB78066}", "AudioRender");
            known.Add("{05589FAF-C356-11CE-BF01-00AA0055595A}", "AudioProperties");
            known.Add("{79376820-07D0-11CF-A24D-0020AFD79767}", "DSoundRender");
            known.Add("{E30629D2-27E5-11CE-875D-00608CB78066}", "AudioRecord");
            known.Add("{2CA8CA52-3C3F-11D2-B73D-00C04FB6BD3D}", "AudioInputMixerProperties");
            known.Add("{CF49D4E0-1115-11CE-B03A-0020AF0BA770}", "AVIDec");
            known.Add("{A888DF60-1E90-11CF-AC98-00AA004C0FA9}", "AVIDraw");
            known.Add("{6A08CF80-0E18-11CF-A24D-0020AFD79767}", "ACMWrapper");
            known.Add("{E436EBB5-524F-11CE-9F53-0020AF0BA770}", "AsyncReader");
            known.Add("{E436EBB6-524F-11CE-9F53-0020AF0BA770}", "URLReader");
            known.Add("{E436EBB7-524F-11CE-9F53-0020AF0BA770}", "PersistMonikerPID");
            known.Add("{5F2759C0-7685-11CF-8B23-00805F6CEF60}", "AMovie");
            known.Add("{D76E2820-1563-11CF-AC98-00AA004C0FA9}", "AVICo");
            known.Add("{8596E5F0-0DA5-11D0-BD21-00A0C911CE86}", "FileWriter");
            known.Add("{E2510970-F137-11CE-8B67-00AA00A3F1A6}", "AviDest");
            known.Add("{C647B5C0-157C-11D0-BD23-00A0C911CE86}", "AviMuxProptyPage");
            known.Add("{0A9AE910-85C0-11D0-BD42-00A0C911CE86}", "AviMuxProptyPage1");
            known.Add("{07B65360-C445-11CE-AFDE-00AA006C14F4}", "AVIMIDIRender");
            known.Add("{187463A0-5BB7-11D3-ACBE-0080C75E246E}", "WMAsfReader");
            known.Add("{7C23220E-55BB-11D3-8B16-00C04FB6BD3D}", "WMAsfWriter");
            known.Add("{AFB6C280-2C41-11D3-8A60-0000F81E0E4A}", "MPEG2Demultiplexer");
            known.Add("{3AE86B20-7BE8-11D1-ABE6-00A0C905F375}", "MMSPLITTER");
            known.Add("{B1B77C00-C3E4-11CF-AF79-00AA00B67A42}", "DVVideoCodec");
            known.Add("{13AA3650-BB6F-11D0-AFB9-00AA00B67A42}", "DVVideoEnc");
            known.Add("{4EB31670-9FC6-11CF-AF6E-00AA00B67A42}", "DVSplitter");
            known.Add("{129D7E40-C10D-11D0-AFB9-00AA00B67A42}", "DVMux");
            known.Add("{060AF76C-68DD-11D0-8FC1-00C04FD9189D}", "SeekingPassThru");
            known.Add("{6E8D4A20-310C-11D0-B79A-00AA003767A7}", "Line21Decoder");
            known.Add("{E4206432-01A1-4BEE-B3E1-3702C8EDC574}", "Line21Decoder2");
            known.Add("{CD8743A1-3736-11D0-9E69-00C04FD7C15B}", "OverlayMixer");
            known.Add("{814B9800-1C88-11D1-BAD9-00609744111A}", "VBISurfaces");
            known.Add("{70BC06E0-5666-11D3-A184-00105AEF9F33}", "WSTDecoder");
            known.Add("{301056D0-6DFF-11D2-9EEB-006008039E37}", "MjpegDec");
            known.Add("{B80AB0A0-7416-11D2-9EEB-006008039E37}", "MJPGEnc");
        }
    }
}
