﻿using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormRebase : GitExtensionsForm
    {
        private readonly TranslationString _currentBranchText = new TranslationString("Current branch:");

        private readonly TranslationString _continueRebaseText = new TranslationString("Continue rebase");
        private readonly TranslationString _solveConflictsText = new TranslationString("Solve conflicts");

        private readonly TranslationString _solveConflictsText2 = new TranslationString(">Solve conflicts<");
        private readonly TranslationString _continueRebaseText2 = new TranslationString(">Continue rebase<");

        private readonly TranslationString _noBranchSelectedText = new TranslationString("Please select a branch");

        private readonly TranslationString _branchUpToDateText =
            new TranslationString("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.");
        private readonly TranslationString _branchUpToDateCaption = new TranslationString("Rebase");

        private readonly string _defaultBranch;

        public FormRebase(string defaultBranch)
            : base(true)
        {
            InitializeComponent();
            Translate();
            _defaultBranch = defaultBranch;
        }

        private void FormRebaseLoad(object sender, EventArgs e)
        {
            var selectedHead = Settings.Module.GetSelectedBranch();
            Currentbranch.Text = _currentBranchText.Text + " " + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = Settings.Module.GetHeads(true, true);

            if (_defaultBranch != null)
                Branches.Text = _defaultBranch;

            Branches.Select();

            splitContainer2.SplitterDistance = Settings.Module.InTheMiddleOfRebase() ? 0 : 70;
            EnableButtons();

            // Honor the rebase.autosquash configuration.
            var autosquashSetting = Settings.Module.GetEffectiveSetting("rebase.autosquash");
            chkAutosquash.Checked = "true" == autosquashSetting.Trim().ToLower();
        }

        private void EnableButtons()
        {
            if (Settings.Module.InTheMiddleOfRebase())
            {
                if (Height < 200)
                    Height = 500;

                Branches.Enabled = false;
                Ok.Enabled = false;

                AddFiles.Enabled = true;
                Resolved.Enabled = !Settings.Module.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = Settings.Module.InTheMiddleOfConflictedMerge();
                Skip.Enabled = true;
                Abort.Enabled = true;
            }
            else
            {
                Branches.Enabled = true;
                Ok.Enabled = true;
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            SolveMergeconflicts.Visible = Settings.Module.InTheMiddleOfConflictedMerge();

            Resolved.Text = _continueRebaseText.Text;
            Mergetool.Text = _solveConflictsText.Text;
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            if (Settings.Module.InTheMiddleOfConflictedMerge())
            {
                AcceptButton = Mergetool;
                Mergetool.Focus();
                Mergetool.Text = _solveConflictsText2.Text;
                MergeToolPanel.BackColor = Color.Black;
            }
            else if (Settings.Module.InTheMiddleOfRebase())
            {
                AcceptButton = Resolved;
                Resolved.Focus();
                Resolved.Text = _continueRebaseText2.Text;
                ContinuePanel.BackColor = Color.Black;
            }
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartResolveConflictsDialog(this);
            EnableButtons();
        }

        private void InteractiveRebaseClick(object sender, EventArgs e)
        {
            chkAutosquash.Enabled = chkInteractive.Checked;
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartAddFilesDialog(this);
        }

        private void ResolvedClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.ContinueRebaseCmd());

            if (!Settings.Module.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void SkipClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.SkipRebaseCmd());

            if (!Settings.Module.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void AbortClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.AbortRebaseCmd());

            if (!Settings.Module.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void OkClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (string.IsNullOrEmpty(Branches.Text))
            {
                MessageBox.Show(this, _noBranchSelectedText.Text);
                return;
            }

            var rebaseCmd = GitCommandHelpers.RebaseCmd(Branches.Text, chkInteractive.Checked, chkPreserveMerges.Checked, chkAutosquash.Checked);
            var dialogResult = FormProcess.ReadDialog(this, rebaseCmd);
            if (dialogResult.Trim() == "Current branch a is up to date.")
                MessageBox.Show(this, _branchUpToDateText.Text, _branchUpToDateCaption.Text);

            if (!Settings.Module.InTheMiddleOfConflictedMerge() &&
                !Settings.Module.InTheMiddleOfRebase() &&
                !Settings.Module.InTheMiddleOfPatch())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void SolveMergeconflictsClick(object sender, EventArgs e)
        {
            MergetoolClick(sender, e);
        }

        private void chkPreserveMerges_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ShowOptions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowOptions.Visible = false;
            OptionsPanel.Visible = true;
            splitContainer2.SplitterDistance = 100;
        }
    }
}
