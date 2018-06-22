using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Utility;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Sitecore.Support.Forms.Shell.UI.Dialogs
{
  public abstract class IDEHtmlEditorBase : Sitecore.Forms.Shell.UI.Dialogs.IDEHtmlEditorBase
  {
    public new string GetValueByKey(string key)
    {
      if (base.nvParams == null)
      {
        base.nvParams = ConvertParameters(ParametersUtil.XmlToNameValueCollection(base.Params));
      }
      return base.nvParams[key] ?? string.Empty;
    }

    protected new NameValueCollection ConvertParameters(NameValueCollection parameters)
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

    protected new string ConvertIdToValue(string valueWithId)
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
      Item item = base.CurrentDatabase.GetItem(value, base.CurrentLanguage);
      if (item == null)
      {
        return value;
      }
      Field field = item.Fields[Sitecore.Form.Core.Configuration.FieldIDs.FieldTitleID];
      if (field == null)
      {
        return value;
      }
      return string.IsNullOrEmpty(field.Value) ? item.Name : item.Fields[Sitecore.Form.Core.Configuration.FieldIDs.FieldTitleID].Value;
    }
  }
}