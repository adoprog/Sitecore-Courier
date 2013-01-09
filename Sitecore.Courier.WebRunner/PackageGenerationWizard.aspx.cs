namespace Sitecore.Courier.WebRunner
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Web.UI.WebControls;

  using Sitecore.Courier;
  using Sitecore.StringExtensions;
  using Sitecore.Update;
  using Sitecore.Update.Interfaces;
  using PackageGenerator = Sitecore.Update.Engine.PackageGenerator;

  public partial class PackageGenerationWizard : System.Web.UI.Page
  {
    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    /// <summary>
    /// Handles the Analyze_ click event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Analyze_Click(object sender, EventArgs e)
    {
      var sourcePath = this.SourcePath.Text;
      var targetPath = this.TargetPath.Text;
      var result = DiffGenerator.GetDiffCommands(sourcePath, targetPath);
      this.Added.Items.Clear();
      this.Changed.Items.Clear();
      this.Deleted.Items.Clear();
      
      foreach (var command in result)
      {
        var item = new ListItem(command.EntityPath.Substring(0, command.EntityPath.IndexOf("{", System.StringComparison.Ordinal) - 1), command.EntityID);
        if (command.CommandPrefix == "addeditems")
        {
          item.Selected = true;
          this.Added.Items.Add(item);
        }

        if (command.CommandPrefix == "changeditems")
        {
          item.Selected = true;
          this.Changed.Items.Add(item);
        }

        if (command.CommandPrefix == "deleteditems")
        {
          item.Selected = true;
          this.Deleted.Items.Add(item);
        }
      }

      AddedCount.Text = Added.Items.Count.ToString(CultureInfo.InvariantCulture);
      DeletedCount.Text = Deleted.Items.Count.ToString(CultureInfo.InvariantCulture);
      ChangedCount.Text = Changed.Items.Count.ToString(CultureInfo.InvariantCulture);

      this.DownloadLink.Visible = false;
      this.AnalyzeResults.Visible = true;
    }

    /// <summary>
    /// Handles the Generate_ click event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Generate_Click(object sender, EventArgs e)
    {
      var sourcePath = this.SourcePath.Text;
      var targetPath = this.TargetPath.Text;

      var result = DiffGenerator.GetDiffCommands(sourcePath, targetPath);

      var fileName = "SitecoreCourier_{0}.update".FormatWith(Guid.NewGuid());
      var filePath = string.Format("{0}/{1}", this.Server.MapPath("/temp"), fileName);

      result = FilterCommands(result);
      var diff = new DiffInfo(result, "Sitecore Courier Package", string.Empty, string.Format("Diff has been generated between '{0}' and '{1}'", this.SourcePath.Text, this.TargetPath.Text));
      PackageGenerator.GeneratePackage(diff, string.Empty, filePath);

      this.DownloadLink.NavigateUrl = string.Format("/temp/{0}", fileName);
      this.DownloadLink.Visible = true;
      this.AnalyzeResults.Visible = false;
    }

    /// <summary>
    /// Executes the filter commands event.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>
    /// Filtered Commands
    /// </returns>
    protected virtual List<ICommand> FilterCommands(List<ICommand> result)
    {
      var enabledCommands = this.Added.Items.Cast<ListItem>().Where(item => item.Selected).Select(item => item.Value).ToList();
      enabledCommands.AddRange(this.Changed.Items.Cast<ListItem>().Where(item => item.Selected).Select(item => item.Value));
      enabledCommands.AddRange(this.Deleted.Items.Cast<ListItem>().Where(item => item.Selected).Select(item => item.Value));

      return result.Where(command => enabledCommands.Contains(command.EntityID)).ToList();
    }
  }
}