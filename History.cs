using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using DirectShowLib;

namespace gep
{
    abstract class HistoryItem
    {
        public abstract string Define(CodeGenBase cg);
        public abstract string Build(CodeGenBase cg);

        public string var, clsname, RealName, Name;
        public string srcFileName, dstFileName;
    }

    abstract class CodeGenBase
    {
        public abstract string DefineAddFilterDS(HIAddFilterDS hi);
        public abstract string BuildAddFilterDS(HIAddFilterDS hi);
        public abstract string BuildAddFilterMon(HIAddFilterMon hi);
        public abstract string Connect(HIConnect hi);
        public abstract string GenCode();

        public bool needCreateFilterProc = false;
        protected History history;
        public History History { set { history = value; } }
        protected Dictionary<string, string> known = new Dictionary<string, string>(); // guid => CLSID_Shit
        protected List<string> srcFileNames = new List<string>();
        protected List<string> dstFileNames = new List<string>();
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

        public override string Build(CodeGenBase cg)
        {
            return cg.BuildAddFilterDS(this);
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

        public override string Build(CodeGenBase cg)
        {
            return cg.BuildAddFilterMon(this);
        }
    }

    class HIConnect : HistoryItem
    {
        public string filter1, filter2, pin1, pin2;
        public int weight = 1;
        public string majortype;

        public HIConnect(string _filter1, string _pin1, string _filter2, string _pin2, Guid type)
        {
            filter1 = _filter1; filter2 = _filter2; pin1 = _pin1; pin2 = _pin2;
            majortype = DsToString.MediaTypeToString(type);
        }

        public override string Define(CodeGenBase cg)
        {
            return "";
        }

        public override string Build(CodeGenBase cg)
        {
            return cg.Connect(this);
        }

    }

    
    class History
    {
        List<HistoryItem> history = new List<HistoryItem>();

        public IEnumerable Items
        {
            get
            {
                foreach (HistoryItem i in history)
                    yield return i;
            }
        }

        public HistoryItem FindByRealName(string realname)
        {
            foreach (HistoryItem i in history)
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

        public void AddFilterIfNew(FilterProps fp, string realname, string srcfilename, string dstfilename)
        {
            if (FindByRealName(realname) == null)
                history.Add(new HIAddFilterDS(fp.FriendlyName, fp.CLSID, realname, fp.FileName, srcfilename, dstfilename, this));
        }

        /*public void Connect(Pin p1, Pin p2) //not used?
        {
            history.Add(new HIConnect(p1.Filter.Name, p1.Name, p2.Filter.Name, p2.Name));
        }*/

        public void ConnectIfNew(Pin p1, Pin p2)
        {
            AMMediaType mt = new AMMediaType();
            p1.IPin.ConnectionMediaType(mt);
            Guid type = mt.majorType;
            DsUtils.FreeAMMediaType(mt);

            if (FindConnection(p1,p2) < 0)
                history.Add(new HIConnect(p1.Filter.Name, p1.Name, p2.Filter.Name, p2.Name, type));
        }

        public void RemoveConnection(Pin p1, Pin p2)
        {
            int i = FindConnection(p1, p2);
            if (i >= 0) history.RemoveAt(i);
        }

        int FindConnection(Pin p1, Pin p2)
        {
            for (int i = 0; i < history.Count; i++)
            {
                HistoryItem hi = history[i];
                HIConnect hc = hi as HIConnect;
                if (hc != null && hc.filter1 == p1.Filter.Name && hc.filter2 == p2.Filter.Name
                               && hc.pin1 == p1.Name && hc.pin2 == p2.Name)
                    return i;
            }
            return -1;
        }

        public void RemoveFilter(string realname)
        {
            for (int i = 0; i < history.Count;i++ )
                if (history[i].RealName == realname)
                {
                    history.RemoveAt(i);
                    break;
                }
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

        public void SortConnections()
        {
            foreach (HistoryItem hi in history) //init weights
            {
                HIConnect hc = hi as HIConnect;
                if (hc != null)
                    hc.weight = 1;
            }
            int i;
            for (i = 0; i < history.Count-1; i++) //calc weights
            {
                HIConnect hc = history[i] as HIConnect;
                if (hc != null)
                {
                    for (int j = i + 1; j < history.Count; j++)
                    {
                        HIConnect hc2 = history[j] as HIConnect;
                        if (hc2 != null)
                        {
                            if (hc2.filter2 == hc.filter1)
                                hc.weight = Math.Max(hc.weight, hc2.weight + 1);
                        }
                    }
                }
            }
            i = 0; //sort
            while (i < history.Count - 1) 
            {
                HIConnect hc = history[i] as HIConnect;
                int k = -1;
                if (hc != null)
                {                    
                    for (int j = i + 1; j < history.Count; j++)
                    {
                        HIConnect hc2 = history[j] as HIConnect;
                        if (hc2 != null)
                        {
                            if (hc2.weight < hc.weight)
                                k = j;    
                        }
                    }
                    if (k >= 0) //somethin found
                    {
                        history.RemoveAt(i);
                        history.Insert(k, hc);
                    }
                }
                if (k < 0) i++; //no moves done, go next                    
            }
        }


    }


    class CodeGenCPP : CodeGenBase
    {
        public CodeGenCPP()
        {
            InitGuidsTable();
        }

        public override string DefineAddFilterDS(HIAddFilterDS hi)
        {            
            StringBuilder sb = new StringBuilder();
            string guid = hi.CLSID, s;
            if (known.TryGetValue(guid, out s))
            {
                hi.clsname = s;
                return ""; //no need to define
            }
            sb.AppendLine("// " + guid);
            sb.Append("DEFINE_GUID(");
            sb.Append(hi.clsname);
            sb.AppendLine(",");            
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
            sb.Append("); //"); sb.Append(hi.FileName); sb.AppendLine();
            return sb.ToString();            
        }

        public override string BuildAddFilterDS(HIAddFilterDS hi)
        {
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("    //add " + hi.Name);
            sb2.AppendLine("    CComPtr<IBaseFilter> " + hi.var + ";");
            sb2.AppendLine("    hr = " + hi.var + ".CoCreateInstance(" + hi.clsname + ");");
            sb2.AppendLine("    CHECK_HR(hr, \"Can't create " + hi.Name + "\");");
            sb2.AppendLine(Insert(hi));
            return sb2.ToString();
        }

        public override string BuildAddFilterMon(HIAddFilterMon hi)
        {
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("    //add " + hi.Name);
            sb2.AppendLine("    CComPtr<IBaseFilter> " + hi.var + " = CreateFilter(L\"" + hi.DisplayName.Replace("\\","\\\\") + "\");");
            sb2.Append(Insert(hi));
            return sb2.ToString();
        }


        string Insert(HistoryItem hi)
        {
            StringBuilder sb3 = new StringBuilder();
            sb3.AppendLine("    hr = pGraph->AddFilter(" + hi.var + ", L\"" + hi.Name + "\");");
            sb3.AppendLine("    CHECK_HR(hr, \"Can't add " + hi.Name + " to graph\");");
            if (hi.srcFileName != null)
            {
                string srcvar = hi.var + "_src";
                srcFileNames.Add(hi.srcFileName);
                int n = srcFileNames.Count;
                string srcfnvar = "srcFile" + n.ToString();
                sb3.AppendLine("    //set source filename");
	            sb3.AppendLine("    CComQIPtr<IFileSourceFilter, &IID_IFileSourceFilter> "+srcvar+"("+hi.var+");");
	            sb3.AppendLine("    if (!"+srcvar+")");
		        sb3.AppendLine("        CHECK_HR(E_NOINTERFACE, \"Can't get IFileSourceFilter\");");
	            sb3.AppendLine("    hr = "+srcvar+"->Load("+srcfnvar+", NULL);");
	            sb3.AppendLine("    CHECK_HR(hr, \"Can't load file\");");
            }
            if (hi.dstFileName != null)
            {
                string dstvar = hi.var + "_sink";
                dstFileNames.Add(hi.dstFileName);
                int n = dstFileNames.Count;
                string dstfnvar = "dstFile" + n.ToString();
                sb3.AppendLine("    //set destination filename");
                sb3.AppendLine("    CComQIPtr<IFileSinkFilter, &IID_IFileSinkFilter> " + dstvar + "(" + hi.var + ");");
                sb3.AppendLine("    if (!" + dstvar + ")");
                sb3.AppendLine("        CHECK_HR(E_NOINTERFACE, \"Can't get IFileSinkFilter\");");
                sb3.AppendLine("    hr = " + dstvar + "->SetFileName(" + dstfnvar + ", NULL);");
                sb3.AppendLine("    CHECK_HR(hr, \"Can't set filename\");");
            }

            return sb3.ToString();
        }

        public override string Connect(HIConnect hi)
        {
            HistoryItem h1 = history.FindByRealName(hi.filter1);
            HistoryItem h2 = history.FindByRealName(hi.filter2);
            string var1 = (h1 != null) ? h1.var : "?";
            string var2 = (h2 != null) ? h2.var : "?";
            StringBuilder sb = new StringBuilder();
            string pair = h1.Name + " and " + h2.Name;
            sb.AppendLine("    //connect " + pair);
            sb.AppendLine("    hr = pBuilder->RenderStream(NULL, &MEDIATYPE_" + hi.majortype + ", " + var1 + ", NULL, " + var2 + ");");
            sb.AppendLine("    CHECK_HR(hr, \"Can't connect "+pair+"\");");
            return sb.ToString();
        }

        public override string GenCode()
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
            sb.AppendLine();
            sb.AppendLine("BOOL hrcheck(HRESULT hr, TCHAR* errtext)");
            sb.AppendLine("{");
            sb.AppendLine("    if (hr >= S_OK)");
            sb.AppendLine("        return FALSE;");
            sb.AppendLine("    TCHAR szErr[MAX_ERROR_TEXT_LEN];");
            sb.AppendLine("    DWORD res = AMGetErrorText(hr, szErr, MAX_ERROR_TEXT_LEN);");
            sb.AppendLine("    if (res)");
            sb.AppendLine("        printf(\"Error %x: %s\\n%s\\n\",hr, errtext,szErr);");
            sb.AppendLine("    else");
            sb.AppendLine("        printf(\"Error %x: %s\\n\", hr, errtext);");
            sb.AppendLine("    return TRUE;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("//change this macro to fit your style of error handling");
            sb.AppendLine("#define CHECK_HR(hr, msg) if (hrcheck(hr, msg)) return hr;");
            sb.AppendLine();

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
            sb.AppendLine(sb_def.ToString());

            history.SortConnections();

            StringBuilder sb_bld = new StringBuilder();
            foreach (HistoryItem hi in history.Items)
                sb_bld.AppendLine(hi.Build(this));

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
        public CodeGenCS()
        {
            InitGuidsTable();
        }

        public override string DefineAddFilterDS(HIAddFilterDS hi)
        {
            StringBuilder sb = new StringBuilder();
            string guid = hi.CLSID, s;
            if (known.TryGetValue(guid, out s))
            {
                hi.clsname = s;
                return ""; //no need to define
            }
            sb.Append("            Guid ");
            sb.Append(hi.clsname);
            sb.Append(" = new Guid(\"");
            sb.Append(guid);
            sb.Append("\"); //"); 
            sb.Append(hi.FileName); 
            sb.AppendLine();
            return sb.ToString();
        }

        public override string BuildAddFilterDS(HIAddFilterDS hi)
        {
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("            //add " + hi.Name);
            sb2.Append("            IBaseFilter " + hi.var + " = ");
            string s;
            if (known.TryGetValue(hi.CLSID, out s))
            {
                sb2.AppendLine("new " + s + "();");
            }
            else
            {
                sb2.AppendLine("(IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(" + hi.clsname + "));");
            }
            sb2.AppendLine(Insert(hi));
            return sb2.ToString();
        }

        string Insert(HistoryItem hi)
        {
            StringBuilder sb3 = new StringBuilder();
            sb3.AppendLine("            hr = pGraph.AddFilter(" + hi.var + ", \"" + hi.Name + "\");");
            sb3.AppendLine("            checkHR(hr, \"Can't add " + hi.Name + " to graph\");");
            if (hi.srcFileName != null)
            {
                string srcvar = hi.var + "_src";
                srcFileNames.Add(hi.srcFileName);
                int n = srcFileNames.Count;
                string srcfnvar = "srcFile" + n.ToString();
	            sb3.AppendLine("            //set source filename");
                sb3.AppendLine("            IFileSourceFilter " + srcvar + " = " + hi.var + " as IFileSourceFilter;");
                sb3.AppendLine("            if (" + srcvar + " == null)");
                sb3.AppendLine("                checkHR(unchecked((int)0x80004002), \"Can't get IFileSourceFilter\");");
                sb3.AppendLine("            hr = " + srcvar + ".Load(" + srcfnvar + ", null);");
                sb3.AppendLine("            checkHR(hr, \"Can't load file\");");
            }
            if (hi.dstFileName != null)
            {
                string dstvar = hi.var + "_sink";
                dstFileNames.Add(hi.dstFileName);
                int n = dstFileNames.Count;
                string dstfnvar = "dstFile" + n.ToString();
                sb3.AppendLine("            //set destination filename");
                sb3.AppendLine("            IFileSinkFilter " + dstvar + " = " + hi.var + " as IFileSinkFilter;");
                sb3.AppendLine("            if (" + dstvar + " == null)");
                sb3.AppendLine("                checkHR(unchecked((int)0x80004002), \"Can't get IFileSinkFilter\");");
                sb3.AppendLine("            hr = " + dstvar + ".SetFileName(" + dstfnvar + ", null);");
                sb3.AppendLine("            checkHR(hr, \"Can't set filename\");");
            }
            return sb3.ToString();
        }

        public override string BuildAddFilterMon(HIAddFilterMon hi)
        {
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("            //add " + hi.Name);
            sb2.AppendLine("            IBaseFilter " + hi.var + " = CreateFilter(@\"" + hi.DisplayName + "\");");
            sb2.AppendLine(Insert(hi));
            return sb2.ToString();
        }

        public override string Connect(HIConnect hi)
        {
            HistoryItem h1 = history.FindByRealName(hi.filter1);
            HistoryItem h2 = history.FindByRealName(hi.filter2);
            string var1 = (h1 != null) ? h1.var : "?";
            string var2 = (h2 != null) ? h2.var : "?";
            StringBuilder sb = new StringBuilder();
            string pair = h1.Name + " and " + h2.Name;
            sb.AppendLine("            //connect " + pair);
            sb.AppendLine("            hr = pBuilder.RenderStream(null, MediaType." + hi.majortype + ", " + var1 + ", null, " + var2 + ");");
            sb.AppendLine("            checkHR(hr, \"Can't connect " + pair + "\");");
            sb.AppendLine();
            return sb.ToString();
        }

        public override string GenCode()
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
            sb.AppendLine("        static void checkHR(int hr, string msg)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (hr < 0)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine(msg);");
            sb.AppendLine("                DsError.ThrowExceptionForHR(hr);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

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
                //if (def.Length > 0) sb_def.AppendLine();
            }

            history.SortConnections();

            StringBuilder sb_bld = new StringBuilder();
            foreach (HistoryItem hi in history.Items)
                sb_bld.Append(hi.Build(this));

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
                sb.AppendLine("            catch");
                sb.AppendLine("            {");
                sb.AppendLine("                throw;");
                sb.AppendLine("            }");
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
