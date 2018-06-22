using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Configuration;
using Sitecore.Form.Core.Utility;
using Sitecore.Forms.Core.Data;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Pipelines;
using Sitecore.Shell.Applications.Layouts.IDE.Editors.HTML;
using Sitecore.Shell.Controls.RichTextEditor.Pipelines.LoadRichTextContent;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Sitecore.Forms.Shell.UI.Dialogs
{
  public abstract class IDEHtmlEditorBase : IDEHtmlEditorForm
  {
    protected new Border HtmlEditorPane;

    protected NameValueCollection nvParams;

    public Database CurrentDatabase => Factory.GetDatabase(Sitecore.Web.WebUtil.GetQueryString("db"));

    public string CurrentID => Sitecore.Web.WebUtil.GetQueryString("id");

    public string Params => HttpContext.Current.Session[Sitecore.Web.WebUtil.GetQueryString("params")] as string;

    public FormItem FormItem => new FormItem(CurrentDatabase.GetItem(CurrentID, CurrentLanguage));

    public Language CurrentLanguage => Language.Parse(Sitecore.Web.WebUtil.GetQueryString("la"));

    public void SetLongValue(string key, string value)
    {
      string sessionKey = SessionUtil.GetSessionKey();
      HttpContext.Current.Session[sessionKey] = value;
      SetValue(key, sessionKey);
    }

    public string GetValueByKey(string key)
    {
      if (nvParams == null)
      {
        nvParams = ConvertParameters(ParametersUtil.XmlToNameValueCollection(Params));
      }
      return nvParams[key] ?? string.Empty;
    }

    public void SetValue(string key, string value)
    {
      if (nvParams == null)
      {
        nvParams = StringUtil.GetNameValues(Params, '=', '&');
      }
      nvParams[key] = value;
    }

    protected NameValueCollection ConvertParameters(NameValueCollection parameters)
    {
      string[] array = "to|cc|bcc|from|subject".Split('|');
      foreach (string name in array)
      {
        string value = ConvertIdToValue(parameters.Get(name));
        if (!string.IsNullOrEmpty(value))
        {
          parameters.Set(name, value);
        }
      }
      return parameters;
    }

    protected string ConvertIdToValue(string valueWithId)
    {
      if (!string.IsNullOrEmpty(valueWithId))
      {
        valueWithId = Regex.Replace(valueWithId, "\\{([^}]*)\\}", ReplaceEvaluator, RegexOptions.IgnoreCase | RegexOptions.Singleline);
      }
      return valueWithId;
    }

    private string ReplaceEvaluator(Match match)
    {
      Assert.IsNotNull(match, "match");
      string value = match.Value;
      if (!ID.IsID(value))
      {
        return value;
      }
      Item item = CurrentDatabase.GetItem(value, CurrentLanguage);
      if (item == null)
      {
        return value;
      }
      Field field = item.Fields[Sitecore.Form.Core.Configuration.FieldIDs.FieldTitleID];
      if (field == null)
      {
        return value;
      }
      string text = string.IsNullOrEmpty(field.Value) ? item.Name : item.Fields[Sitecore.Form.Core.Configuration.FieldIDs.FieldTitleID].Value;
      return string.Join(string.Empty, "[", text, "]");
    }

    protected override void InitializeEditor()
    {
      string sessionString = Sitecore.Web.WebUtil.GetSessionString("hdl");
      Sitecore.Web.WebUtil.RemoveSessionValue("hdl");
      string uniqueID = Control.GetUniqueID("H");
      UrlString urlString = new UrlString("/sitecore/shell/Controls/Rich Text Editor/Default.aspx");
      urlString.Append("hdl", uniqueID);
      urlString.Append("da", Context.Database.Name);
      urlString.Append("us", Context.User.Name);
      urlString.Append("la", Sitecore.Web.WebUtil.GetQueryString("la"));
      urlString.Append("so", Sitecore.Web.WebUtil.GetQueryString("so", "/sitecore/system/Settings/Html Editor Profiles/Rich Text Mail"));
      urlString.Append("id", Sitecore.Web.WebUtil.GetQueryString("id"));
      urlString.Append("mo", "Editor");
      urlString.Append("sc_hidetrace", "1");
      urlString.Append("sc_hideprof", "1");
      base.Editor.SourceUri = urlString.ToString();
      LoadRichTextContentArgs loadRichTextContentArgs = new LoadRichTextContentArgs(sessionString);
      PipelineFactory.GetPipeline("loadRichTextContent").Start(loadRichTextContentArgs);
      base.Header = string.Empty;
      base.Footer = string.Empty;
      HttpContext.Current.Session[uniqueID] = loadRichTextContentArgs.Content;
      base.HtmlEditor.Value = loadRichTextContentArgs.Content;
      base.HtmlCrc = Crc.CRC(loadRichTextContentArgs.Content);
      string path = Sitecore.Web.WebUtil.GetQueryString("so", "/sitecore/system/Settings/Html Editor Profiles/Rich Text Mail") + "/Buttons/HTML View";
      if (Context.Database.GetItem(path) == null)
      {
        base.HtmlButton.Visible = false;
      }
      HtmlEditorPane.Controls.Add(new Literal("<input ID=\"__Field\" Type=\"hidden\" value=\"" + GetFields() + "\" />"));
    }

    protected string GetFields()
    {
      FormItem formItem = new FormItem(CurrentDatabase.GetItem(CurrentID, CurrentLanguage));
      StringBuilder stringBuilder = new StringBuilder();
      FieldItem[] fields = formItem.Fields;
      foreach (FieldItem fieldItem in fields)
      {
        if (!string.IsNullOrEmpty(fieldItem.Title))
        {
          stringBuilder.AppendFormat("{0}={1}&", fieldItem.ID, fieldItem.Title);
        }
      }
      if (stringBuilder.Length <= 0)
      {
        return string.Empty;
      }
      return stringBuilder.ToString(0, stringBuilder.Length - 1);
    }

    protected abstract void SaveValues();
  }
}