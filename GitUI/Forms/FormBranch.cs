﻿using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormBranch : GitExtensionsForm
    {
        private readonly TranslationString _selectOneRevision = new TranslationString("Select 1 revision to create the branch on.");
        private readonly TranslationString _branchCaption = new TranslationString("Branch");

        public FormBranch()
            : base(true)
        {
            InitializeComponent();
            Translate();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {

                if (RevisionGrid.GetSelectedRevisions().Count != 1)
                {
                    MessageBox.Show(this, _selectOneRevision.Text, _branchCaption.Text);
                    return;
                }

                string cmd = GitCommandHelpers.BranchCmd(BName.Text, RevisionGrid.GetSelectedRevisions()[0].Guid, CheckoutAfterCreate.Checked);
                FormProcess.ShowDialog(this, cmd);

                Close();

            }
            catch
            {
            }
        }

        private void Checkout_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCheckoutBranchDialog(this);
            MergeConflictHandler.HandleMergeConflicts(this);
            RevisionGrid.RefreshRevisions();
        }

        private void FormBranch_Load(object sender, EventArgs e)
        {
            RevisionGrid.Load();

            BName.Focus();
            AcceptButton = Ok;
        }
    }
}