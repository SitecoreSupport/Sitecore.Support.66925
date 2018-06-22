using Sitecore.Diagnostics;
using Sitecore.Form.Core.Utility;
using Sitecore.Forms.Core.Data;
using Sitecore.StringExtensions;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;
using Sitecore.WFFM.Core.Resources;
using System;
using System.Text.RegularExpressions;

namespace Sitecore.Support.Forms.Shell.UI.Dialogs
{
  public class SendMailEditor : Sitecore.Support.Forms.Shell.UI.Dialogs.IDEHtmlEditorBase
  {
    public static readonly string ccKey = "CC";

    public static readonly string bccKey = "BCC";

    public static readonly string mailKey = "Mail";

    public static readonly string subjectKey = "Subject";

    public static readonly string toKey = "To";

    public static readonly string fromKey = "From";

    public static readonly string localFromKey = "LocalFrom";

    protected Button Cancel;

    protected Edit CC;

    protected Edit BCC;

    protected Button OK;

    protected Edit Subject;

    protected Edit To;

    protected Edit From;

    protected GenericControl ToLink;

    protected GenericControl FromLink;

    protected GenericControl CCLink;

    protected GenericControl SubjectLink;

    protected Image ToMenuImg;

    protected Image FromMenuImg;

    protected Image CCMenuImg;

    protected Image SubjectMenuImg;

    protected Literal ToLabel;

    protected Literal FromLabel;

    protected Literal CCLabel;

    protected Literal BCCLabel;

    protected Literal SubjectLabel;

    protected ContextMenu ToContextMenu;

    protected ContextMenu FromContextMenu;

    protected ContextMenu CCContextMenu;

    protected ContextMenu SubjectContextMenu;

    protected XmlControl Dialog;

    public string AllowedToTypes => Sitecore.Web.WebUtil.GetQueryString("AllowedToTypes", "{84ABDA34-F9B1-4D3A-A69B-E28F39697069}");

    public string AllowedFromTypes => Sitecore.Web.WebUtil.GetQueryString("AllowedFromTypes", "{84ABDA34-F9B1-4D3A-A69B-E28F39697069}");

    public string AllowedCCTypes => Sitecore.Web.WebUtil.GetQueryString("AllowedCCTypes", "{84ABDA34-F9B1-4D3A-A69B-E28F39697069}");

    public string AllowedSubjectTypes => Sitecore.Web.WebUtil.GetQueryString("AllowedSubjectTypes", string.Empty);

    public string MailValue
    {
      get
      {
        return base.GetValueByKey(mailKey);
      }
      set
      {
        base.SetValue(mailKey, value);
      }
    }

    public string SubjectValue
    {
      get
      {
        return base.GetValueByKey(subjectKey);
      }
      set
      {
        base.SetValue(subjectKey, value);
      }
    }

    public string BCCValue
    {
      get
      {
        return base.GetValueByKey(bccKey);
      }
      set
      {
        base.SetValue(bccKey, value);
      }
    }

    public string CCValue
    {
      get
      {
        return base.GetValueByKey(ccKey);
      }
      set
      {
        base.SetValue(ccKey, value);
      }
    }

    public string ToValue
    {
      get
      {
        return base.GetValueByKey(toKey);
      }
      set
      {
        base.SetValue(toKey, value);
      }
    }

    public string FromValue
    {
      get
      {
        return base.GetValueByKey(fromKey);
      }
      set
      {
        base.SetValue(fromKey, value);
      }
    }

    public string LocalFromValue
    {
      get
      {
        return base.GetValueByKey(localFromKey);
      }
      set
      {
        base.SetValue(localFromKey, value);
      }
    }

    protected override void OnLoad(EventArgs e)
    {
      Sitecore.Web.WebUtil.SetSessionValue("hdl", MailValue);
      base.OnLoad(e);
      if (OK != null)
      {
        OK.OnClick += OnOK;
      }
      if (Cancel != null)
      {
        Cancel.OnClick += OnCancel;
      }
      if (!Context.ClientPage.IsEvent)
      {
        FillContextMenu(ToContextMenu, AllowedToTypes, To, ToLink, ToMenuImg, ToLabel);
        FillContextMenu(FromContextMenu, AllowedFromTypes, From, FromLink, FromMenuImg, FromLabel);
        FillContextMenu(CCContextMenu, AllowedCCTypes, CC, CCLink, CCMenuImg, CCLabel);
        FillContextMenu(SubjectContextMenu, AllowedSubjectTypes, Subject, SubjectLink, SubjectMenuImg, SubjectLabel);
        To.Value = ToValue;
        From.Value = FromValue;
        CC.Value = CCValue;
        BCC.Value = BCCValue;
        Subject.Value = SubjectValue;
        Localize();
        BuildUpClientDictionary();
      }
    }

    protected virtual void BuildUpClientDictionary()
    {
      Context.ClientPage.ClientScript.RegisterClientScriptBlock(base.GetType(), "scWfmRadCommand", "Sitecore.EmailEditor.dictionary['Insert Field'] = \"{0}\";".FormatWith(ResourceManager.Localize("INSERT_FIELD")), true);
    }

    protected virtual void Localize()
    {
      Dialog["Header"] = ResourceManager.Localize("SEND_MAIL_EDITOR");
      Dialog["Text"] = ResourceManager.Localize("CONFIGURE_THE_TEMPLATE_OF_YOUR_MAIL");
      ToLabel.Text = ResourceManager.Localize("TO");
      FromLabel.Text = ResourceManager.Localize("FROM");
      ToMenuImg.Alt = ResourceManager.Localize("INSERT_FIELDS");
      FromMenuImg.Alt = ResourceManager.Localize("INSERT_FIELDS");
      CCLabel.Text = ResourceManager.Localize("CC");
      CCMenuImg.Alt = ResourceManager.Localize("INSERT_FIELDS");
      BCCLabel.Text = ResourceManager.Localize("BCC");
      SubjectLabel.Text = ResourceManager.Localize("SUBJECT");
      SubjectMenuImg.Alt = ResourceManager.Localize("INSERT_FIELDS");
      base.DesignButton.Header = ResourceManager.Localize("DESIGN");
      base.HtmlButton.Header = ResourceManager.Localize("HTML");
    }

    protected virtual void OnOK(object sender, EventArgs args)
    {
      SaveValues();
      string text = ParametersUtil.NameValueCollectionToXml(base.nvParams);
      if (text.Length == 0)
      {
        text = "-";
      }
      SheerResponse.SetDialogValue(text);
      OnCancel(sender, args);
    }

    protected override void SaveValues()
    {
      FieldItem[] fields = base.FormItem.Fields;
      ToValue = ConvertToId(To.Value, fields);
      FromValue = ConvertToId(From.Value, fields);
      CCValue = ConvertToId(CC.Value, fields);
      BCCValue = ConvertToId(BCC.Value, fields);
      LocalFromValue = ConvertToId(From.Value, fields);
      SubjectValue = ConvertToId(Subject.Value, fields);
      base.Update();
      base.SetLongValue(mailKey, base.Body);
    }

    protected string ConvertToId(string value, FieldItem[] fields)
    {
      if (!string.IsNullOrEmpty(value))
      {
        value = Regex.Replace(value, "\\[[^\\]]*\\]", (Match match) => ReplaceEvaluator(match, fields), RegexOptions.IgnoreCase | RegexOptions.Singleline);
      }
      return value;
    }

    private string ReplaceEvaluator(Match match, FieldItem[] fields)
    {
      string a = match.Value.Substring(1, match.Value.Length - 2);
      foreach (FieldItem fieldItem in fields)
      {
        if (a == fieldItem.Title || a == fieldItem.Name)
        {
          return "[" + fieldItem.ID.ToString() + "]";
        }
      }
      return match.Value;
    }

    protected virtual void OnCancel(object sender, EventArgs args)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(args, "args");
      SheerResponse.CloseWindow();
    }

    private void FillContextMenu(ContextMenu menu, string allowedTypes, Edit insertValueTo, GenericControl link, Image img, Literal label)
    {
      FieldItem[] fields = base.FormItem.Fields;
      foreach (FieldItem fieldItem in fields)
      {
        if ((string.IsNullOrEmpty(allowedTypes) || allowedTypes.Contains(fieldItem.TypeID.ToString())) && !string.IsNullOrEmpty(fieldItem.Title))
        {
          menu.Add(fieldItem.Title, string.Empty, "AddValue(\"{0}\", \"{1}\")".FormatWith(fieldItem.Title, insertValueTo.ID));
        }
      }
      if (menu.Controls.Count == 0)
      {
        link.Attributes.Remove("href");
        link.Attributes.Remove("onclick");
        label.Style.Add("margin", "0 22 0 0");
        img.Style.Add("display", "none");
      }
    }

    protected void AddValue(string value, string id)
    {
      string text = string.Join(string.Empty, "[", value, "]");
      switch (id)
      {
        case "Subject":
          {
            Edit subject = Subject;
            Edit edit = subject;
            edit.Value += text;
            SheerResponse.SetOuterHtml(id, Subject);
            break;
          }
        case "CC":
          SmartAdd(CC, text);
          SheerResponse.SetOuterHtml(id, CC);
          break;
        case "From":
          SmartAdd(From, text);
          SheerResponse.SetOuterHtml(id, From);
          break;
        case "To":
          SmartAdd(To, text);
          SheerResponse.SetOuterHtml(id, To);
          break;
      }
      SheerResponse.Eval("scForm.browser.closePopups();if (Sitecore.Forms.PopupMenu.activePopup != null && Sitecore.Forms.PopupMenu.activePopup.parentNode != null) {$(Sitecore.Forms.PopupMenu.activePopup).remove();}");
    }

    private void SmartAdd(Edit edit, string value)
    {
      if (string.IsNullOrEmpty(edit.Value))
      {
        edit.Value = value;
      }
      else if (edit.Value.EndsWith(" "))
      {
        edit.Value += value;
      }
      else if (edit.Value.EndsWith(";"))
      {
        edit.Value = edit.Value + " " + value;
      }
      else
      {
        edit.Value = edit.Value + "; " + value;
      }
    }
  }
}