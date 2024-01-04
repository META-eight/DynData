using Ice.Core;  
using System.Drawing;  
using Infragistics.Shared;  
using Infragistics.Win;  
using Infragistics.Win.UltraWinToolbars;  
using Infragistics.Win.UltraWinGrid;  
using System.Collections;  
using System.Collections.Generic;  
using System.Linq;  
using System.Threading.Tasks;  
using System.IO;  
using System.Text;  
using System.Text.RegularExpressions;  
using System.Reflection;    

#pragma warning disable 0618  

// *** ADD CUSTOM ASSEMBLIES - Ice.Contracts.BO.DynamicQuery.dll ; Ice.Core.Session.dll *** \\  

#region Classes  

// DynData code copyright Meta Eight Ltd 2022 
// Documentation at https://github.com/META-eight/DynData

class DynData //version 2024-01-04
{  
    private bool debugon;  
    private string debuguser;  
    private string baqName;  
    private Ice.Adapters.DynamicQueryAdapter adptr;  
    private Ice.BO.DynamicQueryDataSet ds;  
    private Ice.BO.QueryExecutionDataSet dsBAQ;  
    private EpiDataView edv;  
    private DataTable results;  
    private EpiUltraGrid grid;  
    private Dictionary<string, string> baqParams;  
    private Dictionary<string, string> defParams;  
    private Dictionary<string, string> lastParams;  
    private string[] keynames;  
    private string[] keys;  
    private Dictionary<string, int> colWidths;  
    private Dictionary<string, string> formats;  
    private Dictionary<string, Dictionary<string, Color>> colours;  
    private EpiTransaction oTrans;  
    private Dictionary<string, Control> paramControls;  
    private Dictionary<string, Control> filterControls;  
    private Dictionary<string, string> manualFilters;  
    private Dictionary<string, Dictionary<Control, List<string>>> visibleControls;  
    private List<Control> updControls;  
    private Dictionary<string, Control> gotoControls; 
    private Color paramColour;  
    private bool autoselect;  
    private bool changedParams;  
    private bool gotBAQ;  
    private bool updateable;  
    private string dduserid;  
    private DateTime lastexec;  
    private int refreshlimit;  
    private bool allowmultirow;  
    private bool defaulttolastrow;  
    private bool getdataaftersave;  

    public DynData(string baq, EpiTransaction trans)  
    {  
        debugon = false;  
        debuguser = "XXXXX";  
        dduserid = ((Ice.Core.Session)trans.Session).UserID;  
        baqName = baq;  
        oTrans = trans;  
        adptr = new Ice.Adapters.DynamicQueryAdapter(oTrans);  
        adptr.BOConnect();  
        edv = new EpiDataView();  
        edv.OKToNotifyOthers = false;  
        results = new DataTable();  
        edv.dataView = results.DefaultView;  
        oTrans.Add(baqName, edv);  
        IsDirty = false;  
        autoselect = true;  
        changedParams = true;  
        filterControls = new Dictionary<string, Control>();  
        manualFilters = new Dictionary<string, string>();  
        gotoControls = new Dictionary<string, Control>(); 
        gotBAQ = false;  
        updateable = true;  
        refreshlimit = 3000;  
        allowmultirow = false;  
        defaulttolastrow = false;  
        getdataaftersave = true;  
    }  

    public string Name  
    {  
        get { return baqName; }  
    }  

    public void CloseDown()  
    {  
        if (grid != null)  
        {  
            grid.CellChange -= grid_CellChange;  
            grid.AfterRowActivate -= new System.EventHandler(grid_AfterRowActivate);  
        }  
        if (edv != null) { edv.dataView.ListChanged -= edv_ListChanged; }  
        RemoveFilterEventHandlers();  
        paramControls = null;  
        filterControls = null;  
        manualFilters = null;  
        visibleControls = null;  
        updControls = null;  
        gotoControls = null; 
        colours = null;  
        formats = null;  
        colWidths = null;  
        defParams = null;  
        baqParams = null;  
        results = null;  
        edv = null;  
        dsBAQ = null;  
        ds = null;  
        adptr.Dispose();  
        adptr = null;  
    }  

    public bool Updateable  
    {  
        get  
        {  
            return updateable;  
        }  
        set  
        {  
            updateable = value;  
        }  
    }  

    public bool AutoSelect  
    {  
        get  
        {  
            return autoselect;  
        }   
        set  
        {  
            autoselect = value;  
        }   
    }  

    public int RefreshLimit  
    {  
        get { return refreshlimit; }  
        set { refreshlimit = value; }  
    }  

    public bool IsDirty { get; private set; }  

    public bool AllowMultiRow  
    {  
        get  
        {  
            return allowmultirow;  
        }  
        set  
        {  
            allowmultirow = value;  
        }  
    }  

    public bool DefaultToLastRow  
    {  
        get  
        {  
            return defaulttolastrow;  
        }  
        set  
        {  
            defaulttolastrow = value;  
        }  
    }  

    public bool GetDataAfterSave  
    {  
        get  
        {  
            return getdataaftersave;  
        }  
        set  
        {  
            getdataaftersave = value;  
        }  
    }  

    public EpiDataView GetEdv()  
    {  
        return edv;  
    }  

    public DataTable GetTable()  
    {  
        return results;  
    }  

    public void Clear()  
    {  
        adptr.ClearDynamicQueryData();  
    }  

    public DataRowView CurrentDataRow()  
    {  
        if (edv == null || edv.Row < 0)  
        {  
            return null;  
        }  
        else  
        {  
            return edv.dataView[edv.Row];  
        }  
    }  

    public int RowCount()  
    {  
        int ret = 0;  
        if (edv != null)  
        {  
            ret = edv.dataView.Count;  
        }  
        return ret;  
    }  

    public string[] KeyNames()  
    {  
        return keynames;  
    }  

    public string[] CurrentKeys()  
    {  
        return keys;  
    }  

    public void UpdateKeys(string[] newkeys)  
    {  
        if (keys.Length == newkeys.Length)  
        {  
            keys = newkeys;  
        }  
    }  

    public Dictionary<string, string> CurrentParams()  
    {  
        return baqParams;  
    }  

    public string[] ParamNames()  
    {  
        return baqParams.Keys.ToArray();  
    }  

    public Dictionary<string, string> Params()  
    {  
        return baqParams;  
    }  

    public void UpdateParam(string key, string newval)  
    {  
        if (baqParams.ContainsKey(key))  
        {  
            baqParams[key] = newval;  
            changedParams = true;  
        }  
    }  

    public void ResetParams()  
    {  
        baqParams = new Dictionary<string, string>(defParams);  
        changedParams = true;  
    }  

    public void ParamsFromControls()  
    {  
        if (paramControls != null && grid != null)  
        {  
            Control top = grid;  
            while (top.Parent != null) { top = top.Parent; }  
            foreach (KeyValuePair<string, Control> p in paramControls)  
            {  
                Control c = p.Value;  
                string val = ControlValue(c);  
                string[] bits = p.Key.Split('~');  
                string key = bits[0];  
                if (bits.Length > 1)  
                {  
                    if (bits[1] == "LIKE")  
                    {  
                        val = "%" + val + "%";  
                    }  
                    else if (bits[1] == "LIKEALL")  
                    {  
                        val = val.Replace(" ", "%");  
                    }  
                }   
                if (baqParams[key] != val)  
                {  
                    UpdateParam(key, val);  
                }  
            }  
        }  
    }  

    public Dictionary<string, string> ManualFilters()  
    {  
        return manualFilters;  
    }  

    public void ReplaceManualFilters(Dictionary<string,string> NewFilters)  
    {  
        manualFilters = NewFilters;  
        FilterGrid();  
    }  

    public void ClearManualFilters()  
    {  
        manualFilters.Clear();  
    }  

    private string ControlValue(Control c)  
    {  
        string val = string.Empty;  
        if (c is EpiTextBox)  
        {  
            val = ((EpiTextBox)c).Text ?? ((EpiTextBox)c).Value.ToString() ?? string.Empty;  
        }  
        else if (c is EpiCombo && ((EpiCombo)c).Value != null)  
        {  
            val = ((EpiCombo)c).Value.ToString();  
        }  
        else if (c is EpiCheckBox && ((EpiCheckBox)c).CheckState != CheckState.Indeterminate)  
        {  
            val = ((EpiCheckBox)c).Checked.ToString();  
        }  
        else if (c is BAQCombo && ((BAQCombo)c).Value != null)  
        {  
            val = ((BAQCombo)c).Value.ToString();  
        }  
        else if (c is EpiDateTimeEditor && ((EpiDateTimeEditor)c).Value != null)  
        {  
            val = ((DateTime)((EpiDateTimeEditor)c).Value).ToString("s");  
        }  
        else if (c is EpiTimeEditor)  
        {  
        }  
        else if (c is EpiNumericEditor && ((EpiNumericEditor)c).Value != null)  
        {  
            val = ((EpiNumericEditor)c).Value.ToString();  
        }  
        else if (c is EpiCurrencyEditor && (decimal?)((EpiCurrencyEditor)c).Value != null)  
        {  
            val = ((EpiCurrencyEditor)c).Value.ToString();  
        }  
        else if (c is EpiRetrieverCombo && ((EpiRetrieverCombo)c).Value != null)  
        {  
            val = ((EpiRetrieverCombo)c).Value.ToString();  
        }  
        return val;  
    }  

    public void Initialise(  
        string[] paramnames,  
        string[] defaultparams,  
        string[] keycols,  
        string[] colWnames,  
        int[] colW,  
        string[] colFnames,  
        string[] colF,  
        EpiUltraGrid g)  
    {  
        if (paramnames != null && defaultparams != null && paramnames.Length == defaultparams.Length)  
        {  
            baqParams = new Dictionary<string, string>();  
            defParams = new Dictionary<string, string>();  
            for (int i = 0; i < paramnames.Length; i++)  
            {  
                baqParams[paramnames[i]] = defaultparams[i];  
                defParams[paramnames[i]] = defaultparams[i];  
            }  
        }  
        if (keycols != null)  
        {  
            keynames = (string[])keycols.Clone();  
            keys = new string[keycols.Length];  
        }  
        if (colWnames != null && colW != null && colWnames.Length == colW.Length)  
        {  
            colWidths = new Dictionary<string, int>();  
            for (int i = 0; i < colW.Length; i++)  
            {  
                colWidths[colWnames[i]] = colW[i];  
            }  
        }  
        if (colFnames != null && colF != null && colFnames.Length == colF.Length)  
        {  
            formats = new Dictionary<string, string>();  
            for (int i = 0; i < colF.Length; i++)  
            {  
                formats[colFnames[i]] = colF[i];  
            }  
        }  
        paramColour = Color.Red;  
        bool upd = updateable;
        updateable = false;
        if (g != null)  
        {  
            InitialiseGrid(g);  
        }  
        else  
        {  
            GetData();  
        }  
        updateable = upd;
        edv.dataView.ListChanged += edv_ListChanged;  
        results.ColumnChanged += results_ColumnChanged;  
    }  

    public void AddColourSet(string col, string[] vals, string[] colourhash)  
    {  

        if (!(colours != null)) { colours = new Dictionary<string, Dictionary<string, Color>>(); }  
        if (vals != null && colourhash != null && vals.Length == colourhash.Length)  
        {  
            Dictionary<string, Color> newSet = new Dictionary<string, Color>();  
            for (int i = 0; i < vals.Length; i++)  
            {  
                newSet[vals[i]] = ColorTranslator.FromHtml(colourhash[i]);  
            }  
            colours[col] = newSet;  
        }  
    }  

    public void SetParamWarningColour(Color colourToUse)  
    {  
        paramColour = colourToUse;  
    }  

    public void InitialiseGrid(EpiUltraGrid g)  
    {  
        SetGrid(g);  
        GetData();  
        FormatGrid();  
        MatchDropdowns();  
        FindParamControls();  
        FindFilterControls();  
        FindVisibleControls();  
        FindGotoControls(); 
        grid.UpdateMode = UpdateMode.OnCellChangeOrLostFocus;  
        grid.CellChange += grid_CellChange;  
        grid.AfterRowActivate += new System.EventHandler(grid_AfterRowActivate);  
    }  

    public void SetGrid(EpiUltraGrid g)  
    {  
        grid = g;  
        //grid.SyncWithCurrencyManager = false;  
    }  

    public void FindParamControls()  
    {  
        Control top = grid;  
        while (top.Parent != null) { top = top.Parent; }  
        AddParamControl(top);  
    }  

    private void AddParamControl(Control parentcontrol)  
    {  
        foreach (Control c in parentcontrol.Controls)  
        {  
            if (c.HasChildren)  
            {  
                AddParamControl(c);  
            }  
            else  
            {  
                if (c.Tag != null && c.Tag.ToString() != string.Empty)  
                {  
                    string tag = c.Tag.ToString();  
                    string[] bits = tag.Split(' ');  
                    for (int i = 0; i < bits.Length; i++)  
                    {  
                        if (bits[i].Length > 1 && bits[i].Substring(0,2) == "p:")  
                        {  
                            string[] fbits = bits[i].Substring(2, bits[i].Length - 2).Split('.');  
                            if (fbits.Length == 2 && fbits[0] == baqName)  
                            {  
                                string[] parambits = fbits[1].Split('~');  
                                if (!(paramControls != null)) { paramControls = new Dictionary<string, Control>(); }  
                                paramControls[parambits[0]] = c;  
                            }  
                        }  
                    }  
                }  
            }  
        }   
    }  

    public void FindVisibleControls()  
    {  
        Control top = grid;  
        while (top.Parent != null) { top = top.Parent; }  
        AddControlVisibility(top);  
    }  

    private void AddControlVisibility(Control parentcontrol)  
    {  
        foreach (Control c in parentcontrol.Controls)  
        {  
            bool isvisible = true;  
            if (c.Tag != null && c.Tag.ToString() != string.Empty)  
            {  
                string tag = c.Tag.ToString();  
                string[] bits = tag.Split(' ');  
                for (int i = 0; i < bits.Length; i++)  
                {  
                    if (bits[i].Length > 1 && bits[i].Substring(0,2) == "v:")  
                    {  
                        string[] fbits = bits[i].Substring(2, bits[i].Length - 2).Split(':');  
                        if (fbits.Length == 2 && fbits[0].Substring(0, baqName.Length) == baqName)  
                        {  
                            bool defaultcontrol = false;  
                            if (fbits[1].Length == 0) { defaultcontrol = true; }  
                            string[] truevals = fbits[1].Split('~');  
                            string[] applies = fbits[0].Split('.');  
                            string field = applies[1];  
                            if (visibleControls == null) {visibleControls = new Dictionary<string, Dictionary<Control, List<string>>>(); }  
                            Dictionary<Control, List<string>> controls;  
                            if (visibleControls.ContainsKey(field))  
                            {  
                                controls = visibleControls[field];  
                            }  
                            else  
                            {  
                                controls = new Dictionary<Control, List<string>>();  
                            }  
                            if (defaultcontrol)  
                            {  
                                controls[c] = null;  
                            }  
                            else  
                            {  
                                List<string> vals = new List<string>(truevals);  
                                controls[c] = vals;  
                            }  
                            visibleControls[field] = controls;  
                            isvisible = false;  
                            c.Visible = defaultcontrol;  
                        }  
                    }  
                }  
            }  
            if (c.HasChildren && isvisible)  
            {  
                AddControlVisibility(c);  
            }  
        }  
    }  

    private void SetControlsVisible(string field, string newval)  
    {  
        if (visibleControls != null && visibleControls.ContainsKey(field) && newval != null)  
        {  
            int viscount = 0;  
            Control defc = null;  
            foreach (KeyValuePair<Control, List<string>> vc in visibleControls[field])  
            {  
                if (vc.Value != null && vc.Value.Contains(newval))  
                {  
                    vc.Key.Visible = true;  
                    viscount++;  
                }  
                else  
                {  
                    vc.Key.Visible = false;  
                }  
                if (vc.Value == null) {defc = vc.Key;}  
            }  
            if (viscount == 0 && defc != null)  
            {  
                defc.Visible = true;  
            }  
        }  
    }  

    public void FindFilterControls()  
    {  
        Control top = grid;  
        while (top.Parent != null) { top = top.Parent; }  
        AddFilterControl(top);  
    }  

    private void AddFilterControl(Control parentcontrol)  
    {  
        foreach (Control c in parentcontrol.Controls)  
        {  
            if (c.HasChildren)  
            {  
                AddFilterControl(c);  
            }  
            else  
            {  
                if (c.Tag != null && c.Tag.ToString() != string.Empty)  
                {  
                    string tag = c.Tag.ToString();  
                    string[] bits = tag.Split(' ');  
                    for (int i = 0; i < bits.Length; i++)  
                    {  
                        if (bits[i].Length > 1 && bits[i].Substring(0,2) == "f:")  
                        {  
                            string[] fbits = bits[i].Substring(2, bits[i].Length - 2).Split('.');  
                            if (fbits.Length == 2 && fbits[0] == baqName)  
                            {  
                                //MessageBox.Show(c.Name + " " + fbits[1]);  
                                if (!(filterControls != null)) { filterControls = new Dictionary<string, Control>(); }  
                                filterControls[fbits[1]] = c;  
                                if (c is EpiTextBox)  
                                {  
                                    ((EpiTextBox)c).ValueChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                                else if (c is EpiCombo)  
                                {  
                                    ((EpiCombo)c).ValueChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                                else if (c is EpiCheckBox)  
                                {  
                                    ((EpiCheckBox)c).CheckedChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                                else if (c is BAQCombo)  
                                {  
                                    ((BAQCombo)c).ValueChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                                else if (c is EpiDateTimeEditor)  
                                {  
                                    ((EpiDateTimeEditor)c).ValueChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                                else if (c is EpiTimeEditor)  
                                {  
                                    ((EpiTimeEditor)c).ValueChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                                else if (c is EpiNumericEditor)  
                                {  
                                    ((EpiNumericEditor)c).ValueChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                                else if (c is EpiCurrencyEditor)  
                                {  
                                    ((EpiCurrencyEditor)c).ValueChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                                else if (c is EpiRetrieverCombo)  
                                {  
                                    ((EpiRetrieverCombo)c).ValueChanged += new System.EventHandler(FilterControl_ValueChanged);  
                                }  
                            }  
                        }  
                    }  
                }  
            }  
        }  
    }  

    private void RemoveFilterEventHandlers()  
    {  
        if (filterControls != null)  
        {  
            foreach (Control c in filterControls.Values)  
            {  
                if (c is EpiTextBox)  
                {  
                    ((EpiTextBox)c).ValueChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
                else if (c is EpiCombo)  
                {  
                    ((EpiCombo)c).ValueChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
                else if (c is EpiCheckBox)  
                {  
                    ((EpiCheckBox)c).CheckedChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
                else if (c is BAQCombo)  
                {  
                    ((BAQCombo)c).ValueChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
                else if (c is EpiDateTimeEditor)  
                {  
                    ((EpiDateTimeEditor)c).ValueChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
                else if (c is EpiTimeEditor)  
                {  
                    ((EpiTimeEditor)c).ValueChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
                else if (c is EpiNumericEditor)  
                {  
                    ((EpiNumericEditor)c).ValueChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
                else if (c is EpiCurrencyEditor)  
                {  
                    ((EpiCurrencyEditor)c).ValueChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
                else if (c is EpiRetrieverCombo)  
                {  
                    ((EpiRetrieverCombo)c).ValueChanged -= new System.EventHandler(FilterControl_ValueChanged);  
                }  
            }  
        }  
    }  

    private void FilterControl_ValueChanged(object sender, System.EventArgs args)  
    {  
        FilterGrid();  
        //MessageBox.Show(((Control)sender).Name);  
    }  

    private FilterComparisionOperator FilterComp(string strcomp, ref string repchars)  
    {  
        FilterComparisionOperator comp = FilterComparisionOperator.Equals;  
        switch (strcomp)  
        {  
            case "EQUALS":  
                comp = FilterComparisionOperator.Equals;  
                break;  
            case "CONTAINS":  
                comp = FilterComparisionOperator.Contains;  
                break;  
            case "GREATERTHAN":  
                comp = FilterComparisionOperator.GreaterThan;  
                break;  
            case "LESSTHAN":  
                comp = FilterComparisionOperator.LessThan;  
                break;  
            case "GREATERTHANOREQUALTO":  
                comp = FilterComparisionOperator.GreaterThanOrEqualTo;  
                break;  
            case "LESSTHANOREQUALTO":  
                comp = FilterComparisionOperator.LessThanOrEqualTo;  
                break;  
            case "STARTSWITH":  
                comp = FilterComparisionOperator.StartsWith;  
                break;  
            case "LIKE":  
                comp = FilterComparisionOperator.Like;  
                repchars = "*";  
                break;  
            case "NOTEQUALS":  
                comp = FilterComparisionOperator.NotEquals;  
                break;  
            case "ENDSWITH":  
                comp = FilterComparisionOperator.EndsWith;  
                break;  
            case "DOESNOTCONTAIN":  
                comp = FilterComparisionOperator.DoesNotContain;  
                break;  
            case "MATCH":  
                comp = FilterComparisionOperator.Match;  
                repchars = ".*";  
                break;  
            case "DOESNOTMATCH":  
                comp = FilterComparisionOperator.DoesNotMatch;  
                repchars = ".*";  
                break;  
            case "NOTLIKE":  
                comp = FilterComparisionOperator.NotLike;  
                repchars = "*";  
                break;  
            case "DOESNOTSTARTWITH":  
                comp = FilterComparisionOperator.DoesNotStartWith;  
                break;  
            case "DOESNOTENDWITH":  
                comp = FilterComparisionOperator.DoesNotEndWith;  
                break;  
        }  
        return comp;  
    }  

    private void FilterWorkings(ref string val, ref string[] valbits, string[] keybits, ref FilterComparisionOperator comp, ref FilterLogicalOperator op, ref string repchars)  
    {  
        comp = FilterComparisionOperator.Equals;  
        op = FilterLogicalOperator.And;  
        string strcomp = string.Empty;  
        if (keybits.Length > 1)  
        {  
            strcomp = keybits[1].ToUpper();  
            comp = FilterComp(strcomp, ref repchars);  
        }  
        if (keybits.Length > 2)  
        {  
            string strop = keybits[2].ToUpper();  
            switch (strop)  
            {  
                case "AND":  
                    op = FilterLogicalOperator.And;  
                    break;  
                case "OR":  
                    op = FilterLogicalOperator.Or;  
                    break;  
            }  
        }  
        if (keybits.Length > 3)  
        {  
            if (keybits[3] == "LIKEALL")  
            {  
                string[] likebits = val.Split('"');  
                List<string> keywords = new List<string>();  
                for (int i = 0; i < likebits.Length; i++)  
                {  
                    if ((i % 2) == 1)  
                    {  
                        keywords.Add(likebits[i]);  
                    }  
                    else  
                    {  
                        keywords.AddRange(likebits[i].Split(' '));  
                    }  
                }  
                valbits = keywords.ToArray();  
            }  
        }  
        if (strcomp == "MATCH")  
        {  
            List<string> matchchars = new List<string>( new string[] {" ", ".", "-", ","} );  
            string replacechars = "[ ,.-]?";  
            foreach (string matchchar in matchchars)  
            {  
                val = val.Replace(matchchar, "~~");  
            }  
                val = val.Replace("~~", replacechars);  
        }  
        else if (repchars != " ") { val = repchars + val + repchars; }  
    }  

    public void FilterGrid()  
    {  
        if (grid != null)  
        {  
            UltraGridBand band = grid.DisplayLayout.Bands[0];  
            band.Override.RowFilterMode = RowFilterMode.AllRowsInBand;  
            foreach (ColumnFilter f in band.ColumnFilters)  
            {  
                f.FilterConditions.Clear();  
            }  
            foreach (KeyValuePair<string, Control> p in filterControls)  
            {  
                Control c = p.Value;  
                string val = ControlValue(c);  
                string[] valbits = null;  
                string[] bits = p.Key.Split('~');  
                string key = bits[0];  
                string repchars = " ";  
                if (val != string.Empty)  
                {  
                    FilterComparisionOperator comp = FilterComparisionOperator.Equals;  
                    FilterLogicalOperator op = FilterLogicalOperator.And;  
                    FilterWorkings(ref val, ref valbits, bits, ref comp, ref op, ref repchars);  
                    band.Columns[key].AllowRowFiltering = DefaultableBoolean.False;  
                    if (c is EpiTextBox)  
                    {  
                        if (valbits != null)  
                        {  
                            for (int i = 0; i < valbits.Length; i++)  
                            {  
                                if (repchars != " ") { valbits[i] = repchars + valbits[i] + repchars; }  
                                band.ColumnFilters[key].FilterConditions.Add(comp, valbits[i]);  
                            }  
                        }  
                        else  
                        {  
                            band.ColumnFilters[key].FilterConditions.Add(comp, val);  
                        }  
                            band.ColumnFilters[key].LogicalOperator = op;  
                    }  
                    else if (c is EpiCombo && ((EpiCombo)c).Value != null)  
                    {  
                        band.ColumnFilters[key].FilterConditions.Add(comp, ((EpiCombo)c).Value);  
                        band.ColumnFilters[key].LogicalOperator = op;  
                    }  
                    else if (c is EpiCheckBox && ((EpiCheckBox)c).CheckState != CheckState.Indeterminate)  
                    {  
                        band.ColumnFilters[key].FilterConditions.Add(comp, ((EpiCheckBox)c).Checked);  
                        band.ColumnFilters[key].LogicalOperator = op;  
                    }  
                    else if (c is BAQCombo && ((BAQCombo)c).Value != null)  
                    {  
                        band.ColumnFilters[key].FilterConditions.Add(comp, ((BAQCombo)c).Value);  
                        band.ColumnFilters[key].LogicalOperator = op;  
                    }  
                    else if (c is EpiDateTimeEditor && ((EpiDateTimeEditor)c).Value != null)  
                    {  
                        band.ColumnFilters[key].FilterConditions.Add(comp, ((DateTime)((EpiDateTimeEditor)c).Value));  
                        band.ColumnFilters[key].LogicalOperator = op;  
                    }  
                    else if (c is EpiTimeEditor)  
                    {  
                    }  
                    else if (c is EpiNumericEditor && ((EpiNumericEditor)c).Value != null)  
                    {  
                        band.ColumnFilters[key].FilterConditions.Add(comp, ((EpiNumericEditor)c).Value);  
                        band.ColumnFilters[key].LogicalOperator = op;  
                    }  
                    else if (c is EpiCurrencyEditor && (decimal?)((EpiCurrencyEditor)c).Value != null)  
                    {  
                        band.ColumnFilters[key].FilterConditions.Add(comp, ((EpiCurrencyEditor)c).Value);  
                        band.ColumnFilters[key].LogicalOperator = op;  
                    }  
                    else if (c is EpiRetrieverCombo && ((EpiRetrieverCombo)c).Value != null)  
                    {  
                        band.ColumnFilters[key].FilterConditions.Add(comp, ((EpiRetrieverCombo)c).Value);  
                        band.ColumnFilters[key].LogicalOperator = op;  
                    }  
                }  
            }  
            foreach (KeyValuePair<string, string> f in manualFilters)  
            {  
                string val = f.Value;  
                string[] valbits = null;  
                string[] bits = f.Key.Split('~');  
                string key = bits[0];  
                string repchars = " ";  
                if (val != string.Empty)  
                {  
                    FilterComparisionOperator comp = FilterComparisionOperator.Equals;  
                    if (bits.Length > 1)  
                    {  
                        comp = FilterComp(bits[1], ref repchars);  
                    }  
                    FilterLogicalOperator op = FilterLogicalOperator.And;  
                    FilterWorkings(ref val, ref valbits, bits, ref comp, ref op, ref repchars);  
                    if (valbits != null)  
                    {  
                        for (int i = 0; i < valbits.Length; i++)  
                        {  
                            if (repchars != " ") { valbits[i] = repchars + valbits[i] + repchars; }  
                            band.ColumnFilters[key].FilterConditions.Add(comp, valbits[i]);  
                        }  
                    }  
                    else  
                    {  
                        band.ColumnFilters[key].FilterConditions.Add(comp, val);  
                    }  
                    if (bits.Length > 2) op = FilterLogicalOperator.Or;  
                    band.ColumnFilters[key].LogicalOperator = op;  
                    //MessageBox.Show(key + " ... " + op.ToString());  
                }  
            }  
            FilterEventArgs args = new FilterEventArgs();  
            OnFilteredChange(args);  
        }  
    }  

    public void ResetFilters()  
    {  
        Control top = grid;  
        while (top.Parent != null) { top = top.Parent; }  
        foreach (KeyValuePair<string, Control> p in filterControls)  
        {  
            Control c = p.Value;  
            string val = string.Empty;  
            if (c.Tag != null && c.Tag.ToString() != string.Empty)  
            {  
                string tag = c.Tag.ToString();  
                string[] bits = tag.Split(' ');  
                for (int i = 0; i < bits.Length; i++)  
                {  
                    if (bits[i].Length > 1 && bits[i].Substring(0,2) == "p:")  
                    {  
                        string[] fbits = bits[i].Substring(2, bits[i].Length - 2).Split('.');  
                        if (fbits.Length == 2 && fbits[0] == baqName)  
                        {  
                            string[] parambits = fbits[1].Split('~');  
                            val = defParams[parambits[0]] ?? string.Empty;  
                        }  
                        break;  
                    }  
                }  
            }  
            if (c is EpiTextBox)  
            {  
                ((EpiTextBox)c).Text = val;  
            }  
            else if (c is EpiCombo)  
            {  
                ((EpiCombo)c).Value = val;  
            }  
            else if (c is EpiCheckBox)  
            {  
                if (val == string.Empty)  
                {  
                    ((EpiCheckBox)c).CheckState = CheckState.Indeterminate;  
                }  
                else  
                {  
                    bool boolval = false;  
                    if (Boolean.TryParse(val, out boolval))  
                    {  
                        ((EpiCheckBox)c).Checked = boolval;  
                    }  
                }  
            }  
            else if (c is BAQCombo)  
            {  
                ((BAQCombo)c).Value = val;  
            }  
            else if (c is EpiDateTimeEditor)  
            {  
                if (val == string.Empty)  
                {  
                    ((EpiDateTimeEditor)c).Value = null;  
                }  
                else  
                {  
                    DateTime d = DateTime.Now.Date;  
                    if (DateTime.TryParse(val, out d))  
                    {  
                        ((EpiDateTimeEditor)c).Value = d;  
                    }  
                }  
            }  
            else if (c is EpiTimeEditor)  
            {  
            }  
            else if (c is EpiNumericEditor)  
            {  
                if (val == string.Empty)  
                {  
                    ((EpiNumericEditor)c).Value = null;  
                }  
                else  
                {  
                    double d = 0.0;  
                    if (Double.TryParse(val, out d))  
                    {  
                        ((EpiNumericEditor)c).Value = d;  
                    }  
                }  
            }  
            else if (c is EpiCurrencyEditor)  
            {  
                if (val == string.Empty)  
                {  
                    ((EpiCurrencyEditor)c).Value = 0.0M;;  
                }  
                else  
                {  
                    decimal d = 0.0M;  
                    if (Decimal.TryParse(val, out d))  
                    {  
                        ((EpiCurrencyEditor)c).Value = d;  
                    }  
                }  
            }  
            else if (c is EpiRetrieverCombo)  
            {  
                ((EpiRetrieverCombo)c).Value = val;  
            }  
        }  
    }  

    public void FindGotoControls()  
    {  
        Control top = grid;  
        while (top.Parent != null) { top = top.Parent; }  
        AddGotoControl(top);  
    }  

    private void AddGotoControl(Control parentcontrol)  
    {  
        foreach (Control c in parentcontrol.Controls)  
        {  
            if (c.HasChildren)  
            {  
                AddGotoControl(c);  
            }  
            else  
            {  
                if (c.Tag != null && c.Tag.ToString() != string.Empty)  
                {  
                    string tag = c.Tag.ToString();  
                    string[] bits = tag.Split(' ');  
                    for (int i = 0; i < bits.Length; i++)  
                    {  
                        if (bits[i].Length > 1 && bits[i].Substring(0,2) == "g:")  
                        {  
                            string[] fbits = bits[i].Substring(2, bits[i].Length - 2).Split('.');  
                            if (fbits.Length == 2 && fbits[0] == baqName)  
                            {  
                                //MessageBox.Show(c.Name + " " + fbits[1]);  
                                if (!(filterControls != null)) { filterControls = new Dictionary<string, Control>(); }  
                                filterControls[fbits[1]] = c;  
                                if (c is EpiTextBox)  
                                {  
                                    ((EpiTextBox)c).ValueChanged += new System.EventHandler(GotoControl_ValueChanged);  
                                }  
                                else if (c is EpiCombo)  
                                {  
                                    ((EpiCombo)c).ValueChanged += new System.EventHandler(GotoControl_ValueChanged);  
                                } 
                                else if (c is BAQCombo)  
                                {  
                                    ((BAQCombo)c).ValueChanged += new System.EventHandler(GotoControl_ValueChanged);  
                                }   
                            }  
                        }  
                    }  
                }  
            }  
        }  
    }  

    private void RemoveGotoEventHandlers()  
    {  
        if (filterControls != null)  
        {  
            foreach (Control c in filterControls.Values)  
            {  
                if (c is EpiTextBox)  
                {  
                    ((EpiTextBox)c).ValueChanged -= new System.EventHandler(GotoControl_ValueChanged);  
                }  
                else if (c is EpiCombo)  
                {  
                    ((EpiCombo)c).ValueChanged -= new System.EventHandler(GotoControl_ValueChanged);  
                } 
                else if (c is BAQCombo)  
                {  
                    ((BAQCombo)c).ValueChanged -= new System.EventHandler(GotoControl_ValueChanged);  
                }  
            }  
        }  
    }  

    private void GotoControl_ValueChanged(object sender, System.EventArgs args)  
    {  
        string tag = (string)((Control)sender).Tag; 
        string[] bits = tag.Substring(2,tag.Length - 2).Split('.'); 
        string colname = string.Empty; 
        if (bits.Length == 2 && bits[0] == baqName)  
        { 
            colname = bits[1]; 
        } 
        string p = ControlValue((Control)sender);  
        p = OnlyAlphaNumeric(p);  
        List<string> ids = new List<string>();  
        bool ishidden = false;  
        foreach (UltraGridRow row in grid.Rows.GetFilteredInNonGroupByRows())  
        {  
            if (OnlyAlphaNumeric(row.Cells[colname].Value.ToString()).StartsWith(p,StringComparison.OrdinalIgnoreCase))  
            {  
                foreach (string k in KeyNames())  
                {  
                    ids.Add(row.Cells[k].Value.ToString());  
                }  
                break;  
            }  
        }  
        if (ids.Count > 0)  
        {  
            GoToRow(ids.ToArray());  
            if (grid.ActiveRow != null)  
            {  
                ScrollRowToQuarterDown(grid.ActiveRow);  
            }  
        }  
    } 

    public void ResetGotos()  
    {  
        Control top = grid;  
        while (top.Parent != null) { top = top.Parent; }  
        foreach (KeyValuePair<string, Control> p in gotoControls)  
        {  
            Control c = p.Value;  
            string val = string.Empty;  
            if (c.Tag != null && c.Tag.ToString() != string.Empty)  
            {  
                string tag = c.Tag.ToString();  
                string[] bits = tag.Split(' ');  
                for (int i = 0; i < bits.Length; i++)  
                {  
                    if (bits[i].Length > 1 && bits[i].Substring(0,2) == "g:")  
                    {  
                        string[] fbits = bits[i].Substring(2, bits[i].Length - 2).Split('.');  
                        if (fbits.Length == 2 && fbits[0] == baqName)  
                        {   
                            val = string.Empty;  
                        }  
                        break;  
                    }  
                }  
            }  
            if (c is EpiTextBox)  
            {  
                ((EpiTextBox)c).Text = val;  
            }  
            else if (c is EpiCombo)  
            {  
                ((EpiCombo)c).Value = val;  
            }  
            else if (c is BAQCombo)  
            {  
                ((BAQCombo)c).Value = val;  
            }  
        } 
    }  

    public bool Save()  
    {  
        bool ret = true;  
        try  
        {  
            oTrans.PushStatusText("Saving " + baqName + "...",true);  
            DataSet retds = adptr.Update(adptr.DynamicQueryData, results.DataSet, false);  
            if (retds != null && retds.Tables.Count > 0 && retds.Tables["Errors"] != null && retds.Tables["Errors"].Rows.Count > 0)  
            {  
                string msg = "Save Error " + baqName;  
                foreach (DataRow row in retds.Tables["Errors"].Rows)  
                {  
                    msg = msg + System.Environment.NewLine + row["ErrorText"].ToString();  
                }  
                MessageBox.Show(msg);  
                ret = false;  
            }  
            else  
            {  
                if (getdataaftersave) { GetData(); }  
            }  
        }  
        catch (Exception e)  
        {  
            MessageBox.Show(e.Message, "Save Error " + baqName);  
            ret = false;  
        }  
        finally  
        {  
            oTrans.PopStatus();  
        }  
        return ret;  
    }  

    public bool Save(bool refreshkeys)  
    {  
        bool ret = true;  
        if (refreshkeys && keynames != null && keys != null && keynames.Length == keys.Length )  
        {  
            for (int i = 0; i < keys.Length; i++)  
            {  
                if (edv != null && edv.Row > -1)  
                {  
                    keys[i] = edv.dataView[edv.Row][keynames[i]].ToString();  
                }  
                else  
                {  
                    keys[i] = string.Empty;  
                }  
            }  
        }  
        string[] keyscopy = (string[])keys.Clone();  
        ret = Save();  
        GoToRow(keyscopy);  
        return ret;  
    }  

    public DataRowView GetNewRow()  
    {  
        DataRowView ret = null;  
        adptr.GetNew(adptr.DynamicQueryData,true);  
        GoToRow(new string[] {string.Empty});  
        SetDirty();  
        ret = CurrentDataRow();  
        return ret;  
    }  

    public void RunCustomAction(string actionID)  
    {  
        oTrans.PushStatusText("Running Action " + actionID + " for " + baqName + "...",true);
		try
		{
			DataSet retds = adptr.RunCustomAction(baqName,actionID,adptr.QueryResults,true);
			if (retds != null && retds.Tables.Count > 0 && retds.Tables["Errors"] != null && retds.Tables["Errors"].Rows.Count > 0)
			{
				string msg = "Error with Action " + actionID + " for " + baqName;
				foreach (DataRow erow in retds.Tables["Errors"].Rows)
				{
					msg = msg + System.Environment.NewLine + erow["ErrorText"].ToString();
				}
				MessageBox.Show(msg);
			}
		}
		finally
		{
			oTrans.PopStatus();
			GetData();
		}
    }  

    public void RefreshData()  
    {  
        if (baqParams != null && lastParams != null)  
        {  
            bool matched = true;  
            foreach (KeyValuePair<string,string> kp in baqParams)  
            {  
                if (!kp.Value.Equals(lastParams[kp.Key]))  
                {  
                    matched = false;  
                    break;  
                }  
            }  
            if (!matched) { GetData(); }  
        }  
        else  
        {  
            GetData();  
        }  
    }  

    public bool ParamsChanged()  
    {  
        bool ret = true;  
        if (baqParams != null && lastParams != null)  
        {  
            bool matched = true;  
            foreach (KeyValuePair<string,string> kp in baqParams)  
            {  
            if (!kp.Value.Equals(lastParams[kp.Key]))  
            {  
                matched = false;  
                break;  
            }  
            }  
            ret = !matched;  
        }  
            return ret;  
    }  

    public void GetData()  
    {  
        if (!(lastexec != null)) { lastexec = DateTime.Now; }  
        if (DateTime.Now.Subtract(lastexec).TotalMilliseconds < refreshlimit && !(ParamsChanged())) { return; }  
        Stopwatch sw = new Stopwatch();  
        string plist = string.Empty;  
        if (debugon && debuguser == dduserid) { sw.Start(); }  
        string[] keyscopy = (string[])keys.Clone();  
        List<string[]> allkeys = new List<string[]>();  
        if (results != null)  
        {  
            foreach (DataRow row in results.Rows)  
            {  
                List<string> rowkeys = new List<string>();  
                for (int i = 0; i < keynames.Length; i++)  
                {  
                    rowkeys.Add(row[keynames[i]].ToString());  
                }  
                allkeys.Add(rowkeys.ToArray());  
            }  
        }  
        bool fresh = false;  
        try  
        {  
            oTrans.PushStatusText("Getting data for " + baqName + "...",true);  
            ParamsFromControls();  
            if (baqName != string.Empty)  
            {  
                if (!gotBAQ)  
                {  
                    if (adptr.GetByID(baqName)) { gotBAQ = true; }  
                }  
                if (!gotBAQ)  
                {  
                    sw.Stop();  
                    return;  
                }  
                if (updateable)  
                {  
                    if (!(ds != null)) { ds = adptr.DynamicQueryData; }  
                    if (ds.DynamicQuery.Rows.Count == 0)  
                    {  
                        fresh = true;  
                        Ice.BO.DynamicQueryDataSet dsQDesign = adptr.QueryDesignData;  
                        DataRow targetRow;  
                        foreach (DataTable table in ds.Tables)  
                        {  
                            foreach (DataRow sourceRow in dsQDesign.Tables[table.ToString()].Rows)  
                            {  
                                targetRow = table.NewRow();  
                                targetRow.ItemArray = sourceRow.ItemArray;  
                                table.Rows.Add(targetRow);  
                            }  
                        }  
                    }  
                    if (!(dsBAQ != null)) { dsBAQ = adptr.GetQueryExecutionParameters(ds); }  
                }  
                else  
                {  
                    if (!(dsBAQ != null)) { dsBAQ = adptr.GetQueryExecutionParametersByID(baqName); }  
                }  
                if (baqParams != null)  
                {  
                    int i = 0;  
                    foreach (KeyValuePair<string, string> p in baqParams)  
                    {  
                        bool empty = false;  
                        string key = p.Key;  
                        string val = p.Value;  
                        if (key.Substring(0,1) == "-")  
                        {  
                            if (val == string.Empty) { empty = true; }  
                            key = key.Substring(1, key.Length - 1);  
                        }  
                        dsBAQ.ExecutionParameter[i].ParameterID = key;  
                        dsBAQ.ExecutionParameter[i].IsEmpty = empty;  
                        dsBAQ.ExecutionParameter[i].ParameterValue = val;  
                        if (debugon && debuguser == dduserid) { plist = plist + key + " - " + val + System.Environment.NewLine; }  
                        i++;  
                    }  
                    dsBAQ.AcceptChanges();  
                }  
                if (updateable)  
                {  
                    adptr.Execute(ds, dsBAQ);  
                }  
                else  
                {  
                    adptr.ExecuteByID(baqName, dsBAQ);  
                }  
                if (adptr.QueryResults != null && adptr.QueryResults.Tables.Count > 0)  
                {  
                    results = adptr.QueryResults.Tables["Results"];  
                }  
                else  
                {  
                    results = new DataTable();  
                }  
                if (!(edv != null)) { edv = (EpiDataView)oTrans.EpiDataViews[baqName]; }  
                if (!(edv != null))  
                {  
                    edv = new EpiDataView();  
                    oTrans.Add(baqName, edv);  
                }  
                edv.dataView = results.DefaultView;  
                if (grid != null) { grid.DataSource = results; }  
                //if (fresh)  
                //{  
                UnlockData();  
                //}  
                GoToGridRow();  
                GoToViewRow(changedParams);  
                changedParams = false;  
                if (edv != null && !(grid != null) && edv.Row == -1 && edv.dataView.Count > 0)  
                {  
                    if (allkeys.Count > 0)  
                    {  
                        foreach (DataRow row in results.Rows)  
                        {  
                            bool matched = false;  
                            List<string> rowkeys = new List<string>();  
                            for (int i = 0; i < keynames.Length; i++)  
                            {  
                                rowkeys.Add(row[keynames[i]].ToString());  
                            }  
                            foreach (string[] akey in allkeys)  
                            {  
                                if (akey.SequenceEqual(rowkeys.ToArray()))  
                                {  
                                    matched = true;  
                                    break;  
                                }  
                            }  
                            if (!matched)  
                            {  
                                GoToRow(rowkeys.ToArray());  
                                break;  
                            }  
                        }  
                    }  
                    if (edv.Row == -1)  
                    {  
                        if (defaulttolastrow)  
                        {  
                            edv.Row = edv.dataView.Count - 1;  
                        }  
                        else  
                        {  
                            edv.Row = 0;  
                        }  
                    }  
                }  
                FormatRows();  
                FormatParamControls();  
            }  
            if (baqParams != null) { lastParams = new Dictionary<string,string>(baqParams); }  
            CheckDirty(true);  
            GetDataEventArgs dargs = new GetDataEventArgs();  
            dargs.GotData = true;  
            OnGetData(dargs);  
            FilterEventArgs fargs = new FilterEventArgs();  
            OnFilteredChange(fargs);  
            if (debugon && debuguser == dduserid)  
            {  
                if (true)  
                {  
                    MessageBox.Show(baqName + System.Environment.NewLine + plist + System.Environment.NewLine + sw.Elapsed.ToString());  
                }  
                sw.Stop();  
            }               
        }  
        finally  
        {  
            oTrans.PopStatus();  
        }  
    }  

    private void MatchGridsToGridReadOnly(Control parentcontrol)  
    {  
        if (grid != null)  
        {  
            if (parentcontrol == null)  
            {  
                parentcontrol = grid.Parent;  
                while (parentcontrol.Parent != null)  
                {  
                    parentcontrol = parentcontrol.Parent;  
                }  
            }  
            foreach (Control c in parentcontrol.Controls)  
            {  
                if (c.GetType().ToString().Replace("Ice.Lib.Framework.", "") == "EpiUltraGrid")  
                {  
                    if (((EpiUltraGrid)c).EpiBinding == baqName)  
                    {  
                        UltraGridBand band1 = grid.DisplayLayout.Bands[0];  
                        UltraGridBand band2 = ((EpiUltraGrid)c).DisplayLayout.Bands[0];  
                        foreach (UltraGridColumn col in band1.Columns)  
                        {  
                            if (band2.Columns.Exists(col.Key))  
                            {  
                                band2.Columns[col.Key].CellActivation = col.CellActivation;  
                            }  
                        }  
                    }  
                }  
                else if (c.HasChildren)  
                {  
                    MatchGridsToGridReadOnly(c);  
                }  
            }  
        }  
    }  

    public void FormatGrid()  
    {  
        if (grid != null)  
        {  
            grid.UpdateMode = UpdateMode.OnCellChange;  
            grid.UseOsThemes = DefaultableBoolean.False;  
            grid.StyleSetName = string.Empty;  
            grid.DrawFilter = null;  
            grid.StyleLibraryName = string.Empty;  
            Color headcolor = Color.FromArgb(80,80,80);  
            Color textcolor = Color.White;  
            grid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;  
            UltraGridBand band = grid.DisplayLayout.Bands[0];  
            for (int i = 0; i < band.Columns.Count; i++)  
            {  
                int width = 0;  
                string f = string.Empty;  
                string caption = band.Columns[i].Header.Caption;  
                if (colWidths != null && colWidths.TryGetValue(caption, out width))  
                {  
                    band.Columns[i].Hidden = false;  
                    band.Columns[i].Width = width;  
                }  
                else  
                {  
                    band.Columns[i].Hidden = true;  
                }  
                if ( formats != null && formats.TryGetValue(caption, out f))  
                {  
                    band.Columns[i].Format = f;  
                }  
                if (band.Columns[i].Key.Contains("Btn"))  
                {  
                    band.Columns[i].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.Button; //EditButton;  
                    band.Columns[i].ButtonDisplayStyle = Infragistics.Win.UltraWinGrid.ButtonDisplayStyle.Always;  
                    band.Columns[i].Header.Caption = string.Empty;  
                }  
            }  
            grid.DisplayLayout.Override.HeaderAppearance.BackColor = headcolor;  
            grid.DisplayLayout.Bands[0].Override.HeaderAppearance.BackColor = headcolor;  
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = textcolor;  
            grid.DisplayLayout.Bands[0].Override.HeaderAppearance.ForeColor = textcolor;  
        }  
    }  

    public void FormatRows(string onlyColumn = "all", string onlyValue = "any")  
    {  
        if (grid != null)  
        {  
            UltraGridBand band = grid.DisplayLayout.Bands[0];  
            foreach (UltraGridRow row in grid.Rows)  
            {  
                if (colours != null && (onlyColumn == "all" || row.Cells[onlyColumn].Value.ToString() == onlyValue))  
                {  
                    foreach (var cset in colours)  
                    {  
                        string colkey = cset.Key;  
                        string colval = string.Empty;  
                        string coltocolour = string.Empty;  
                        if (colkey.Contains("~"))  
                        {  
                            string[] bits = colkey.Split('~');  
                            colval = bits[0];  
                            coltocolour = bits[1];  
                        }  
                        else  
                        {  
                            colval = colkey;  
                            coltocolour = colkey;  
                        }  
                        if (band.Columns.Exists(colval))  
                        {  
                            Color backcolor;  
                            string status = row.Cells[colval].Value.ToString();  
                            if (cset.Value.TryGetValue(status, out backcolor))  
                            {  
                                Color forecolor = (PerceivedBrightness(backcolor) > 140 ? Color.Black : Color.White);  
                                if (coltocolour != string.Empty)  
                                {  
                                    if (coltocolour=="All")  
                                    {  
                                        row.Appearance.BackColor = backcolor;  
                                        row.Appearance.ForeColor = forecolor;  
                                        foreach (UltraGridCell c in row.Cells)  
                                        {  
                                            c.Appearance.BackColor = backcolor;  
                                            c.Appearance.ForeColor = forecolor;  
                                        }  
                                    }                                  
                                    else if (band.Columns.Exists(coltocolour))                     
                                    {  
                                        row.Cells[coltocolour].Appearance.BackColor = backcolor;  
                                        row.Cells[coltocolour].Appearance.ForeColor = forecolor;  
                                    }  
                                }  
                                else  
                                {  
                                    row.Appearance.BackColor = backcolor;  
                                    row.Appearance.ForeColor = forecolor;  
                                }  
                            }  
                        }  
                    }  
                }  
                for (int i = 0; i < band.Columns.Count; i++)  
                {  
                    //if (band.Columns[i].Key.Contains("Btn"))  
                    //{  
                    // row.Cells[band.Columns[i].Key].Value = band.Columns[i].Header.Caption;  
                    //}  
                    if (band.Columns[i].Header.Caption.Contains(" Colour"))  
                    {  
                        string colkey = band.Columns[i].Header.Caption.Replace(" Colour", "");  
                        if (colkey == "All")  
                        {  
                            Color backcolor;  
                            try  
                            {  
                                backcolor = ColorTranslator.FromHtml(row.Cells[band.Columns[i].Key].Value.ToString());  
                            }  
                            catch (Exception e)  
                            {  
                                backcolor = Color.White;  
                            }  
                            Color forecolor = (PerceivedBrightness(backcolor) > 140 ? Color.Black : Color.White);  
                            row.Appearance.BackColor = backcolor;  
                            row.Appearance.ForeColor = forecolor;  
                            foreach (UltraGridCell c in row.Cells)  
                            {  
                                c.Appearance.BackColor = backcolor;  
                                c.Appearance.ForeColor = forecolor;  
                            }  
                        }  
                        else if (band.Columns.Exists(colkey))  
                        {  
                            Color backcolor;  
                            try  
                            {  
                                backcolor = ColorTranslator.FromHtml(row.Cells[band.Columns[i].Key].Value.ToString());  
                            }  
                            catch (Exception e)  
                            {  
                                backcolor = Color.White;  
                            }  
                            Color forecolor = (PerceivedBrightness(backcolor) > 140 ? Color.Black : Color.White);  
                            row.Cells[colkey].Appearance.BackColor = backcolor;  
                            row.Cells[colkey].Appearance.ForeColor = forecolor;  
                        }  
                    }  
                }  
            }  
        }  
    }  

    private int PerceivedBrightness(Color c)  
    {  
        return (int)Math.Sqrt( (c.R * c.R * 0.299) + (c.G * c.G * 0.587) + (c.B * c.B * 0.114) );  
    }  

    private void FormatParamControls()  
    {  
        if (paramControls != null && grid != null)  
        {  
            Control top = grid;  
            while (top.Parent != null) { top = top.Parent; }  
            Color forecolor = (PerceivedBrightness(paramColour) > 140 ? Color.Black : Color.White);  
            foreach (KeyValuePair<string, Control> p in paramControls)  
            {  
                Control c = p.Value;  
                string val = ControlValue(c);  
                if (val != string.Empty)  
                {  
                    c.BackColor = paramColour;  
                    c.ForeColor = forecolor;  
                }  
                else  
                {  
                    c.BackColor = Color.White;  
                    c.ForeColor = Color.Black;  
                }  
            }  
        }  
    }  

    public void MatchDropdowns()  
    {  
        if (grid != null)  
        {  
            UltraGridBand listBand = grid.DisplayLayout.Bands[0];  
            for (int i = 0; i < listBand.Columns.Count; i++)  
            {  
                if (!listBand.Columns[i].Hidden)  
                {  
                    string caption = listBand.Columns[i].Header.Caption;  
                    string key = listBand.Columns[i].Key;  
                    Control top = grid;  
                    while (top.Parent != null) { top = top.Parent; }  
                    MatchDropdownControls(top, listBand, caption, key);  
                }  
            }  
        }  
    }  

    private bool MatchDropdownControls(Control parentcontrol, UltraGridBand listBand, string caption, string key)  
    {  
        bool donebind = false;  
        foreach (Control c in parentcontrol.Controls)  
        {  
            string ctype = c.GetType().ToString().Replace("Ice.Lib.Framework.", "");  
            if (ctype == "BAQCombo" || ctype == "EpiCombo")  
            {  
                if (ctype == "BAQCombo")  
                {  
                    if (((BAQCombo)c).EpiBinding == baqName + "." + key)  
                    {  
                        listBand.Columns[key].ValueList = (BAQCombo)c;  
                        listBand.Columns[key].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;  
                        ((BAQCombo)c).ForceRefreshList();  
                        donebind = true;  
                        break;  
                    }  
                    //break;  
                }  
                else if (ctype == "EpiCombo")  
                {  
                    if (((EpiCombo)c).EpiBinding == baqName + "." + key)  
                    {  
                        listBand.Columns[key].ValueList = (EpiCombo)c;  
                        listBand.Columns[key].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;  
                        ((EpiCombo)c).ForceRefreshList();  
                        donebind = true;  
                        break;  
                    }  
                    //break;  
                }  
            }  
            else if (c.HasChildren)  
            {  
                if (MatchDropdownControls(c, listBand, caption, key))  
                {  
                    return true;  
                }  
            }  
        }  
        return donebind;  
    }  

    public void LockData()  
    {  
        if (edv != null)  
        {  
            foreach (DataColumn col in edv.dataView.Table.Columns)  
            {  
                //col.ExtendedProperties["ReadOnly"] = true;  
                if (grid.DisplayLayout.Bands[0].Columns.Exists(col.ColumnName))  
                {  
                    grid.DisplayLayout.Bands[0].Columns[col.ColumnName].CellActivation = Activation.NoEdit;  
                }  
            }  
            MatchGridsToGridReadOnly(null);  
            if (updControls != null)  
            {  
                foreach (Control c in updControls)  
                {  
                    c.Enabled = false;  
                }  
            }  
        }  
    }  

    public void UnlockData()  
    {  
        if (ds != null && edv != null)  
        {  
            bool setcontrols = false;  
            List<string> updfields = new List<string>();  
            if (updControls == null)  
            {  
                updControls = new List<Control>();  
                setcontrols = true;  
            }  
            foreach (DataColumn col in edv.dataView.Table.Columns)  
            {  
                col.ExtendedProperties["ReadOnly"] = false;  
                if (grid != null && grid.DisplayLayout.Bands[0].Columns.Exists(col.ColumnName))  
                {  
                    grid.DisplayLayout.Bands[0].Columns[col.ColumnName].CellActivation = Activation.AllowEdit;  
                }  
            }  
            foreach (DataRow row in ds.QueryField.Rows)  
            {  
                string rowname = row["Alias"].ToString();  
                bool isreadonly = !(bool)row["Updatable"];  
                if (edv.dataView.Table.Columns.Contains(rowname))  
                {  
                    edv.dataView.Table.Columns[rowname].ExtendedProperties["ReadOnly"] = isreadonly;  
                    if (setcontrols && !isreadonly) { updfields.Add(baqName + "." + rowname); }  
                }  
                if (grid != null && grid.DisplayLayout.Bands[0].Columns.Exists(rowname))  
                {  
                    if (isreadonly)
					{
						grid.DisplayLayout.Bands[0].Columns[rowname].CellActivation = Activation.NoEdit;
					}
					else
					{
						grid.DisplayLayout.Bands[0].Columns[rowname].CellActivation = Activation.AllowEdit;
					} 
                }  
            }  
            if (setcontrols && grid != null)  
            {  
                Control topcontrol = grid.Parent;  
                while (topcontrol.Parent != null) { topcontrol = topcontrol.Parent; }  
                GetUpdateableControls(topcontrol, updfields);  
                string updReport = baqName + System.Environment.NewLine;  
                foreach (Control c in updControls)  
                {  
                    updReport = updReport + c.Name + System.Environment.NewLine;  
                }  
                //MessageBox.Show(updReport + System.Environment.NewLine + string.Join(System.Environment.NewLine, updfields.ToArray()));  
                MatchGridsToGridReadOnly(null);  
                if (updControls != null)  
                {  
                    foreach (Control c in updControls)  
                    {  
                        c.Enabled = true;  
                        if (c.GetType().GetProperty("ReadOnly") != null)  
                        {  
                            PropertyInfo propinfo = c.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(x => x.Name.Equals("readonly", StringComparison.OrdinalIgnoreCase));  
                            if (propinfo != null) { propinfo.SetValue(c, false); }  
                            //MessageBox.Show(c.Name);  
                        }  
                    }  
                }  
            }
        }  
    }  

    private void GetUpdateableControls(Control thiscontrol, List<string> updfields)  
    {  
        string msg = thiscontrol.Name + System.Environment.NewLine;  
        try  
        {  
            if (updfields.Count == 0) return;  
            var type = thiscontrol.GetType();  
            var property = type.GetProperty("EpiBinding");  
            if (property != null && property.GetValue(thiscontrol) != null && updfields.Contains(property.GetValue(thiscontrol).ToString()))  
            {  
                if (!updControls.Contains(thiscontrol))  
                {  
                    updControls.Add(thiscontrol);  
                }  
            }  
            else if (thiscontrol.HasChildren)  
            {  
                foreach (Control c in thiscontrol.Controls)  
                {  
                    GetUpdateableControls(c, updfields);  
                }  
            }  
        }  
        catch (Exception e)  
        {  
            MessageBox.Show(e.Message + System.Environment.NewLine + msg + "GetUpdateableControls");  
        }  
    }  

    public void GoToTopRow()  
    {  
        if (RowCount() > 0)  
        {  
            if (grid != null && grid.Rows.GetFilteredInNonGroupByRows().Count() > 0)  
            {  
                grid.Selected.Rows.Clear();  
                grid.Selected.Rows.Add(grid.Rows.GetFilteredInNonGroupByRows()[0]);  
                grid.ActiveRow = grid.Rows.GetFilteredInNonGroupByRows()[0];  
            }  
            else  
            {  
                edv.Row = 1;  
            }  
            for (int i = 0; i < keys.Length; i++)  
            {  
                if (keys[i] != edv.dataView[edv.Row][keynames[i]].ToString())  
                {  
                    keys[i] = edv.dataView[edv.Row][keynames[i]].ToString();  
                }  
            }  
            GoToRow(keys);  
        }  
    }  

    private UltraGridRow GoToGridRow()  
    {  
        UltraGridRow ret = null;  
        bool multi = false;  
        if ((Control.ModifierKeys == Keys.Shift || Control.ModifierKeys == Keys.Control) && grid.ContainsFocus) { multi = true; }  
        //if (allowmultirow) { MessageBox.Show(baqName + " - " + multi.ToString()); }  
        if (grid != null && keynames != null && keys != null && keynames.Length == keys.Length)  
        {  
            bool rowmatch = false;  
            int rowcount = 0;  
            if (grid.ActiveRow != null)  
            {  
                UltraGridRow row = grid.ActiveRow;  
                if (row.Hidden || row.IsFilteredOut)  
                {  
                    rowmatch = false;  
                }  
                else  
                {  
                    rowmatch = true;  
                    for (int i = 0; i < keys.Length; i++)  
                    {  
                        if (!String.Equals(row.Cells[keynames[i]].Value.ToString(), keys[i], StringComparison.OrdinalIgnoreCase))  
                        {  
                            rowmatch = false;  
                            break;  
                        }  
                    }  
                }  
                if (rowmatch && row != null)  
                {  
                    ret = row;  
                    if (!allowmultirow || !multi) { grid.Selected.Rows.Clear(); }  
                    grid.ActiveRow.Selected = true;  
                }  
            }  
            if (!rowmatch)  
            {  
                foreach (UltraGridRow row in grid.Rows)  
                {  
                    if (row.Hidden || row.IsFilteredOut)  
                    {  
                        rowmatch = false;  
                    }  
                    else  
                    {  
                        rowmatch = true;  
                        rowcount++;  
                        for (int i = 0; i < keys.Length; i++)  
                        {  
                            if (!String.Equals(row.Cells[keynames[i]].Value.ToString(), keys[i], StringComparison.OrdinalIgnoreCase))  
                            {  
                                rowmatch = false;  
                                break;  
                            }  
                        }  
                    }  
                    if (rowmatch && row != null)  
                    {  
                        grid.ActiveRow = row;  
                        ret = row;  
                        if (!allowmultirow || !multi) { grid.Selected.Rows.Clear(); }  
                        grid.ActiveRow.Selected = true;  
                        break;  
                    }  
                }  
                if (!rowmatch)  
                {  
                    if (!allowmultirow || !multi) { grid.Selected.Rows.Clear(); }  
                    if (rowcount == 1 && autoselect)  
                    {  
                        grid.ActiveRow = grid.Rows.GetFilteredInNonGroupByRows()[0];  
                        ret = grid.ActiveRow;  
                        for (int i = 0; i < keys.Length; i++)  
                        {  
                            keys[i] = grid.ActiveRow.Cells[keynames[i]].Value.ToString();  
                        }  
                    }  
                    else  
                    {  
                        grid.ActiveRow = null;  
                        for (int i = 0; i < keys.Length; i++)  
                        {  
                            keys[i] = string.Empty;  
                        }  
                    }  
                }  
            }  
        }  
        return ret;  
    }  

    private DataRowView GoToViewRow(bool newdata = false)  
    {  
        if (allowmultirow && (Control.ModifierKeys == Keys.Shift || Control.ModifierKeys == Keys.Control))  
        {  
            //MessageBox.Show(grid.Selected.Rows.Count.ToString());  
            //MessageBox.Show(Control.ModifierKeys.ToString());  
            edv.Row = -1;  
            return null;  
        }  
        DataRowView ret = null;  
        bool moved = false;  
        bool newsinglerow = false;  
        string[] keycopy = (string[])keys.Clone();  
        if (edv != null && edv.dataView != null && edv.dataView.Count > 0 && keynames != null && keys != null && keynames.Length == keys.Length)  
        {  
            bool rowmatch = false;  
            if (edv.Row > -1 && edv.Row < edv.dataView.Count)  
            {  
                DataRowView row = edv.dataView[edv.Row];  
                rowmatch = true;  
                for (int i = 0; i < keys.Length; i++)  
                {  
                    if (!String.Equals(row[keynames[i]].ToString(), keys[i], StringComparison.OrdinalIgnoreCase))  
                    {  
                        rowmatch = false;  
                        break;  
                    }  
                }  
            }  
            if (!rowmatch)  
            {  
                int rowindex = 0;  
                moved = true;  
                foreach (DataRowView row in edv.dataView)  
                {  
                    rowmatch = true;  
                    for (int i = 0; i < keys.Length; i++)  
                    {  
                        if (!String.Equals(row[keynames[i]].ToString(), keys[i], StringComparison.OrdinalIgnoreCase))  
                        {  
                            rowmatch = false;  
                            break;  
                        }  
                    }  
                    if (rowmatch)  
                    {  
                        edv.Row = rowindex;  
                        break;  
                    }  
                    rowindex++;  
                }  
                if (!rowmatch)  
                {  
                    if (grid != null || edv.dataView.Count == 0)  
                    {  
                        edv.Row = -1;  
                        for (int i = 0; i < keys.Length; i++)  
                        {  
                            keys[i] = string.Empty;  
                        }  
                    }  
                    else  
                    {  
                        edv.Row = 0;  
                        for (int i = 0; i < keys.Length; i++)  
                        {  
                            if (keys[i] != edv.dataView[0][keynames[i]].ToString())  
                            {  
                                newsinglerow = true;  
                                keys[i] = edv.dataView[0][keynames[i]].ToString();  
                            }  
                        }  
                    }  
                }  
            }  
        }  
        if (edv.Row < 0 && defaulttolastrow && edv.dataView.Count > 0)  
        {  
            edv.Row = edv.dataView.Count -1;  
            for (int i = 0; i < keys.Length; i++)  
            {  
                if (keys[i] != edv.dataView[edv.Row][keynames[i]].ToString())  
                {  
                    newsinglerow = true;  
                    keys[i] = edv.dataView[edv.Row][keynames[i]].ToString();  
                }  
            }  
            GoToGridRow();  
        }  
        edv.Notify(new EpiNotifyArgs(oTrans, edv.Row, 0));  
        if (moved || newdata || newsinglerow)  
        {  
            RowEventArgs args = new RowEventArgs();  
            args.edvName = baqName;  
            args.rowindex = edv.Row;  
            args.rowchanged = ArraysEqual<string>(keys, keycopy);  
            OnRowChange(args);  
        }  
        return ret;  
    }  

    private void ScrollRowToQuarterDown(UltraGridRow row)  
    {  
        UltraGrid grid = (UltraGrid)row.Band.Layout.Grid;  
        UIElement gElement = grid.DisplayLayout.UIElement;  
        if (gElement == null) return;  
        UIElement rowColRegionIntersectionUIElement = gElement.GetDescendant(typeof(RowColRegionIntersectionUIElement));  
        if (rowColRegionIntersectionUIElement == null) return;  
        int visrows = rowColRegionIntersectionUIElement.Rect.Height / row.Height;  
        UltraGridRow posrow = row;  
        int roffset = visrows / 4;  
        for (int i=0;i<roffset;i++)  
        {  
            if (posrow.HasPrevSibling(false,true))  
            {  
                posrow = posrow.GetSibling(SiblingRow.Previous,false,true);  
                while (posrow.IsFilteredOut && posrow.HasPrevSibling(false,true))  
                {  
                    posrow = posrow.GetSibling(SiblingRow.Previous,false,true);  
                }  
            }  
        }  
        grid.ActiveRowScrollRegion.FirstRow = posrow;  
    }  

    private string OnlyAlphaNumeric(string input)  
    {  
        char[] carr = input.ToCharArray();  
        carr = Array.FindAll<char>(carr, (c => char.IsLetterOrDigit(c)));  
        return new string(carr);  
    }  
    static bool ArraysEqual<T>(T[] a1, T[] a2)  
    {  
        if (ReferenceEquals(a1,a2)) { return true; }  
        if (a1 == null || a2 == null) { return false; }  
        if (a1.Length != a2.Length) { return false; }  
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;  
        for (int i = 0; i < a1.Length; i++)  
        {  
            if (!comparer.Equals(a1[i], a2[i])) { return false; }  
        }  
        return true;  
    }  

    public bool GoToRow(string[] newkeys)  
    {  
        bool ret = false;  
        try  
        {  
            if (newkeys != null && keys != null && newkeys.Length == keys.Length)  
            {  
                keys = newkeys;  
                UltraGridRow grow = GoToGridRow();  
                DataRowView erow = GoToViewRow();  
                if (grow != null) { ret = true; }  
            }  
        }  
        catch (Exception e)  
        {  
            ret = false;  
        }  
        return ret;  
    }  

    private void grid_AfterRowActivate(object sender, System.EventArgs args)  
    {  
        //if (grid.Selected.Rows.Count == 1)  
        //{  
        AfterGridRowActivate(grid.ActiveRow);  
        //}  
    }  

    public void AfterGridRowActivate(UltraGridRow row)  
    {  
        if (row != null)  
        {  
            string[] keynames = KeyNames();  
            string[] keys = CurrentKeys();  
            for (int i = 0; i < keys.Length; i++)  
            {  
                keys[i] = row.Cells[keynames[i]].Value.ToString();  
            }  
            GoToRow(keys);  
        }  
    }  

    public bool CheckDirty(bool force)  
    {  
        bool nowDirty = false;  
        var chrows = (from DataRow row in results.Rows where row.RowState != DataRowState.Unchanged select row);  
        if (chrows.Count() > 0)  
        {  
            nowDirty = true;  
        }  
        if (nowDirty != IsDirty || force)  
        {  
            IsDirty = nowDirty;  
            DirtyEventArgs args = new DirtyEventArgs();  
            args.IsDirty = IsDirty;  
            OnDirtyChange(args);  
        }  
        return IsDirty;  
    }  

    public void SetDirty()  
    {  
        IsDirty = true;  
        DirtyEventArgs args = new DirtyEventArgs();  
        args.IsDirty = IsDirty;  
        OnDirtyChange(args);  
    }  

    public void SetRowChange()  
    {  
        RowEventArgs args = new RowEventArgs();  
        args.edvName = baqName;  
        args.rowindex = edv.Row;  
        args.rowchanged = false;  
        OnRowChange(args);  
    }  

    private void edv_ListChanged(object sender, ListChangedEventArgs args)  
    {  
        CheckDirty(false);  
    }  

    private void grid_CellChange(object sender, CellEventArgs args)  
    {  
        CheckDirty(false);  
    }  

    private void results_ColumnChanged(object sender, DataColumnChangeEventArgs args)  
    {  
        CheckDirty(false);  
        OnDataColumnChange(args);  
        SetControlsVisible(args.Column.ColumnName, args.ProposedValue.ToString());  
    }  

    protected virtual void OnRowChange(RowEventArgs args)  
    {  
        EventHandler<RowEventArgs> handler = RowChange;  
        if (handler != null) { handler(this, args); }  
        DataRowView row = CurrentDataRow();  
        if (row != null && visibleControls != null)  
        {  
            foreach (string field in visibleControls.Keys)  
            {  
                SetControlsVisible(field, row[field].ToString());  
            }  
        }  
    }  

    protected virtual void OnDirtyChange(DirtyEventArgs args)  
    {  
        EventHandler<DirtyEventArgs> handler = DirtyChange;  
        if (handler != null) { handler(this, args); }  
    }

    protected virtual void OnDataColumnChange(DataColumnChangeEventArgs args)  
    {  
        EventHandler<DataColumnChangeEventArgs> handler = DataColumnChange;  
        if (handler != null && args != null)  
        {  
            if (args.Row.HasVersion(DataRowVersion.Current) && !args.ProposedValue.Equals(args.Row[args.Column.ColumnName, DataRowVersion.Current]))  
            {  
                handler(this,args);  
            }  
        }  
    }  

    protected virtual void OnGetData(GetDataEventArgs args)  
    {  
        EventHandler<GetDataEventArgs> handler = GetNewData;  
        if (handler != null) { handler(this, args); }  
    }  

    protected virtual void OnFilteredChange(FilterEventArgs args)  
    {  
        EventHandler<FilterEventArgs> handler = FilteredChange;  
        if (handler != null) { handler(this, args); }  
    }  

    public event EventHandler<RowEventArgs> RowChange;  
      
    public event EventHandler<DirtyEventArgs> DirtyChange;  

    public event EventHandler<DataColumnChangeEventArgs> DataColumnChange;  

    public event EventHandler<GetDataEventArgs> GetNewData;  

    public event EventHandler<FilterEventArgs> FilteredChange;  
}

class RowEventArgs : EventArgs  
{  
    public string edvName { get; set; }  

    public int rowindex { get; set; }  

    public bool rowchanged { get; set; }  
} 

class DirtyEventArgs : EventArgs  
{  
    public bool IsDirty { get; set; }  
}  

class GetDataEventArgs : EventArgs  
{  
    public bool GotData { get; set; }  
}  

class FilterEventArgs : EventArgs  
{  
}  

class DynDataColumnSelect
{
	private DynData ddParent;
	private EpiTransaction oTrans;
	private UltraGrid pgrid;
	private UltraGrid sgrid;
	private Dictionary<string,int> colwidths;
	private Dictionary<string,bool> colvis;
	private DataTable coltable;

	public DynDataColumnSelect(DynData parent, EpiTransaction trans)
	{
		ddParent = parent;
		oTrans = trans;
		pgrid = ddParent.GetGrid();
		coltable = new DataTable();
		coltable.Columns.Add("Column",typeof(string));
		coltable.Columns.Add("Width",typeof(int));
		coltable.Columns.Add("Show",typeof(bool));
	}

	public void Initialise(
		string[] captions,
		int[] widths,
		bool[] visible,
		UltraGrid setgrid)
	{
		colwidths = new Dictionary<string,int>();
		colvis = new Dictionary<string,bool>();
		if (setgrid != null && coltable != null && widths.Length == captions.Length && visible.Length == captions.Length)
		{
			sgrid = setgrid;
			for (int i=0;i<captions.Length;i++)
			{
				if (!colwidths.Keys.Contains(captions[i]))
				{
					coltable.Rows.Add(captions[i],widths[i],visible[i]);
				}
				colwidths[captions[i]] = widths[i];
				colvis[captions[i]] = visible[i];
			}
			sgrid.DataSource = coltable;
			ColumnsVisible();
			coltable.ColumnChanged += coltable_ColumnChanged;
			sgrid.CellChange += sgrid_CellChange;
		}
	}

	public void CloseDown()
	{
		coltable.ColumnChanged -= coltable_ColumnChanged;
		sgrid.CellChange += sgrid_CellChange;
		coltable = null;
	}

	public void ColumnsVisible()
	{
		UltraGridBand band = pgrid.DisplayLayout.Bands[0];
		foreach (DataRow row in coltable.Rows)
		{
			colwidths[row["Column"].ToString()] = (int?)row["Width"] ?? 0;
			colvis[row["Column"].ToString()] = (bool?)row["Show"] ?? false;
		}
		for (int i=0;i<band.Columns.Count;i++)
		{
			int width = 0;
			bool vis = false;
			string caption = band.Columns[i].Header.Caption;
			if (colwidths.TryGetValue(caption, out width))
			{
				band.Columns[i].Width = width;
			}
			else
			{
				band.Columns[i].Width = 0;
			}
			if (colvis.TryGetValue(caption, out vis))
			{
				band.Columns[i].Hidden = !vis;
			}
			else
			{
				band.Columns[i].Hidden = true;
			}
		}
	}

	private void sgrid_CellChange(object sender, CellEventArgs args)
	{
		if (args.Cell.Column.Key == "Calculated_Checked")
		{
			sgrid.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);
			args.Cell.Row.Update();
			ColumnsVisible();
		}
	}
	
	private void coltable_ColumnChanged(object sender, DataColumnChangeEventArgs args)
	{
		if (args.Column.ColumnName != "Calculated_Checked")
		{
			OnDataColumnChange(args);
			ColumnsVisible();
		}
	}

	protected virtual void OnDataColumnChange(DataColumnChangeEventArgs args)
	{
			EventHandler<DataColumnChangeEventArgs> handler = DataColumnChange;
			if (handler != null && args != null)
			{
				if (args.Row.HasVersion(DataRowVersion.Current) && !args.ProposedValue.Equals(args.Row[args.Column.ColumnName, DataRowVersion.Current]))
				{
					handler(this,args);
				}
			}
	}

	public event EventHandler<DataColumnChangeEventArgs> DataColumnChange;
}

class DynDataFilterGrid  
{  
    private DynData ddParent;  
    private string baqName;  
    private UltraGrid grid;  
    private string keyfield;  
    private string matchfield;  
    private EpiTransaction oTrans;  

    public DynDataFilterGrid(DynData parent, string baq, string displayfield, string parentfield, UltraGrid filtergrid, EpiTransaction trans)  
    {  
        ddParent = parent;  
        baqName = baq;  
        grid = filtergrid;  
        keyfield = displayfield;  
        matchfield = parentfield;  
        oTrans = trans;  
        LoadData();  
        grid.CellChange += grid_CellChange;  
        grid.InitializeLayout += grid_InitializeLayout;  
        ddParent.GetNewData += ddParent_GetData;  
    }  

    public void LoadData()  
    {  
        DynamicQueryAdapter dbaq = new DynamicQueryAdapter(oTrans);  
        dbaq.BOConnect();  
        dbaq.ExecuteByID(baqName);  
        DataTable dtbaq = dbaq.QueryResults.Tables[0];  
        grid.DataSource = dtbaq;  
        UltraGridBand band = grid.DisplayLayout.Bands[0];  
        foreach (UltraGridColumn col in band.Columns)  
        {  
            if (col.Key == keyfield)  
            {  
                col.Hidden = false;  
                col.Width = grid.Width - 90;  
            }  
            else if (col.Key == "Calculated_Checked")  
            {  
                col.Hidden = false;  
                col.Width = 50;  
            }  
            else  
            {  
                col.Hidden = true;  
            }  
        }  
        dbaq.Dispose();  
    }  

    public void CloseDown()  
    {  
        grid.CellChange -= grid_CellChange;  
        grid.InitializeLayout -= grid_InitializeLayout;  
        ddParent.GetNewData -= ddParent_GetData;  
    }  

    private void grid_CellChange(object sender, CellEventArgs args)  
    {  
        if (args.Cell.Column.Key == "Calculated_Checked")  
        {  
            grid.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);  
            args.Cell.Row.Update();  
            UpdateFilters();  
        }  
    }  

    private void grid_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs args)  
    {  
        args.Layout.RowSelectorImages.DataChangedImage = null;  
    }  

    private void ddParent_GetData(object sender, GetDataEventArgs args)  
    {  
        UpdateFilters();  
    }  

    public void UpdateFilters()  
    {  
        bool all = true;  
        List<string> included = new List<string>();  
        foreach (UltraGridRow row in grid.Rows)  
        {  
            if (((bool)row.Cells["Calculated_Checked"].Value))  
            {  
                included.Add(row.Cells[matchfield].Value.ToString());  
            }  
            else all = false;  
        }  
        Dictionary<string,string> newFilters = new Dictionary<string,string>();  
        foreach (KeyValuePair<string,string> oldFilters in ddParent.ManualFilters())  
        {  
            string key = oldFilters.Key.Split('~')[0];  
            if (key != matchfield) newFilters[oldFilters.Key] = oldFilters.Value;  
        }  
        if (all)  
        {  
        }  
        else if (included.Count > 0)  
        {  
            for (int i = 0; i < included.Count; i++)  
            {  
                newFilters[matchfield + "~EQUALS~" + i.ToString("d3")] = included[i];  
            }  
        }  
        else  
        {  
            newFilters[matchfield] = "NOMATCH";  
        }  
        ddParent.ReplaceManualFilters(newFilters);  
    }  
 
    public void ClearAll()  
    {  
        foreach (UltraGridRow row in grid.Rows)  
        {  
            row.Cells["Calculated_Checked"].Value = false;  
        }  
        grid.UpdateData();  
        UpdateFilters();  
    }  

    public void SelectAll()  
    {  
        foreach (UltraGridRow row in grid.Rows)  
        {  
            row.Cells["Calculated_Checked"].Value = true;  
        }  
        grid.UpdateData();  
        UpdateFilters();  
    }  
}  

class DynDataFilterGridSC   
{   
    private DynData ddParent;   
    private UltraGrid grid;   
    private string keyfield;   
    private string matchfield;   
    private EpiTransaction oTrans;    
    private DataTable dt;  
    private bool exblank;  

    public DynDataFilterGridSC(DynData parent, string displayfield, string parentfield, UltraGrid filtergrid, EpiTransaction trans, bool excludeblank = false)   
    {   
        ddParent = parent;   
        grid = filtergrid;   
        keyfield = displayfield;   
        matchfield = parentfield;   
        oTrans = trans;   
        exblank = excludeblank;  
        LoadData();   
        grid.CellChange += grid_CellChange;   
        grid.InitializeLayout += grid_InitializeLayout;   
        ddParent.GetNewData += ddParent_GetData;  
        GetButtons();   
    }   

    private void GetButtons()  
    {  
        string cname = grid.Name.Replace("grd","");  
        foreach (Control c in grid.Parent.Controls)  
        {  
            if (c is EpiButton && c.Name.StartsWith("btn" + cname))  
            {  
                if (c.Name.Equals("btn" + cname + "Clear"))  
                {  
                    c.Click += Click_ClearAll;  
                }  
                else if (c.Name.Equals("btn" + cname + "Select"))  
                {  
                    c.Click += Click_SelectAll;  
                }  
            }  
        }  
    }  

    public void LoadData()   
    {   
        if (keyfield.Equals(matchfield))   
        {   
            dt = ddParent.GetEdv().dataView.ToTable(true,keyfield);   
        }   
        else   
        {   
            dt = ddParent.GetEdv().dataView.ToTable(true,keyfield,matchfield);   
        }  
        if (exblank)  
        {  
            List<DataRow> blanks = new List<DataRow>();  
            foreach (DataRow row in dt.Rows)  
            {  
                if (string.IsNullOrEmpty((string)row[keyfield]))  
                {  
                    blanks.Add(row);  
                }  
            }  
            foreach (var blank in blanks)  
            {  
                dt.Rows.Remove(blank);  
            }  
        }  
        DataColumn showcol = new DataColumn("Show",typeof(bool));   
        showcol.DefaultValue = true;   
        dt.Columns.Add(showcol); //"Show",typeof(bool));   
        grid.DataSource = dt;   
        UltraGridBand band = grid.DisplayLayout.Bands[0];   
        band.ColHeadersVisible = false;   
        foreach (UltraGridColumn col in band.Columns)   
        {   
            if (col.Key == keyfield)   
            {   
                col.Hidden = false;   
                col.Width = grid.Width - 90;   
            }   
            else if (col.Key == "Show")   
            {   
                col.Hidden = false;   
                col.Width = 50;   
            }   
            else   
            {   
                col.Hidden = true;   
            }   
        }   
        band.SortedColumns.Clear();
        band.SortedColumns.Add(keyfield,false,false);
    }   

    public void CloseDown()   
    {   
        grid.CellChange -= grid_CellChange;   
        grid.InitializeLayout -= grid_InitializeLayout;   
        ddParent.GetNewData -= ddParent_GetData;   
        string cname = grid.Name.Replace("grd","");  
        foreach (Control c in grid.Parent.Controls)  
        {  
            if (c is EpiButton && c.Name.StartsWith("btn" + cname))  
            {  
                if (c.Name.Equals("btn" + cname + "Clear"))  
                {  
                    c.Click -= Click_ClearAll;  
                }  
                else if (c.Name.Equals("btn" + cname + "Select"))  
                {  
                    c.Click -= Click_SelectAll;  
                }  
            }  
        }  
    }   

    private void grid_CellChange(object sender, CellEventArgs args)   
    {   
        if (args.Cell.Column.Key == "Show")   
        {   
            grid.PerformAction(Infragistics.Win.UltraWinGrid.UltraGridAction.ExitEditMode);   
            args.Cell.Row.Update();   
            UpdateFilters();   
        }   
    }   

    private void grid_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs args)   
    {   
        args.Layout.RowSelectorImages.DataChangedImage = null;   
    }   

    private void ddParent_GetData(object sender, GetDataEventArgs args)   
    {   
        LoadData();  
        UpdateFilters();   
    }   

    public void UpdateFilters()   
    {   
        try   
        {   
            bool all = true;   
            List<string> included = new List<string>();   
            foreach (UltraGridRow row in grid.Rows)   
            {   
                if (row.Cells["Show"].Value != DBNull.Value && ((bool?)row.Cells["Show"].Value ?? false))   
                {   
                    included.Add(row.Cells[matchfield].Value.ToString());   
                }   
                else all = false;   
            }   
            Dictionary<string,string> newFilters = new Dictionary<string,string>();   
            foreach (KeyValuePair<string,string> oldFilters in ddParent.ManualFilters())   
            {   
                string key = oldFilters.Key.Split('~')[0];   
                if (key != matchfield) newFilters[oldFilters.Key] = oldFilters.Value;   
            }   
            if (all)   
            {   
            }   
            else if (included.Count > 0)   
            {   
                for (int i = 0; i < included.Count; i++)   
                {   
                    newFilters[matchfield + "~EQUALS~" + i.ToString("d3")] = included[i];   
                }   
            }   
            else   
            {   
                newFilters[matchfield] = "NOMATCH";   
            }   
            ddParent.ReplaceManualFilters(newFilters);   
        }   
        catch (Exception e)   
        {   
            string msg = "Error in UpdateFilter: ";   
            while (e != null)   
            {   
                msg = msg + System.Environment.NewLine + e.Message;   
                e = e.InnerException;   
            }   
            MessageBox.Show(msg);   
        }   
    }   
 
    private void Click_ClearAll(object sender, EventArgs args)  
    {  
        ClearAll();  
    }  

    public void ClearAll()   
    {   
        foreach (UltraGridRow row in grid.Rows)   
        {   
            row.Cells["Show"].Value = false;   
        }   
        grid.UpdateData();   
        UpdateFilters();   
    }   

    private void Click_SelectAll(object sender, EventArgs args)  
    {  
        SelectAll();  
    }  

    public void SelectAll()   
    {   
        foreach (UltraGridRow row in grid.Rows)   
        {   
            row.Cells["Show"].Value = true;   
        }   
        grid.UpdateData();   
        UpdateFilters();   
    }   
}  

#endregion 