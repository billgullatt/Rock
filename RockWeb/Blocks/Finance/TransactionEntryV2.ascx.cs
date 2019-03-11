﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Version 2 of the Transaction Entry Block
    /// </summary>
    [DisplayName( "Transaction Entry (V2)" )]
    [Category( "Finance" )]
    [Description( "Creates a new financial transaction or scheduled transaction." )]

    #region Block Attributes

    [FinancialGatewayField(
        "Financial Gateway",
        Key = AttributeKey.FinancialGateway,
        Description = "The payment gateway to use for Credit Card and ACH transactions.",
        Category = AttributeCategory.None,
        Order = 0 )]

    [BooleanField(
        "Enable ACH",
        Key = AttributeKey.EnableACH,
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 1 )]

    [TextField(
        "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch.",
        DefaultValue = "Online Giving",
        Category = AttributeCategory.None,
        Order = 2 )]

    [DefinedValueField(
        "Source",
        Key = AttributeKey.FinancialSourceType,
        Description = "The Financial Source Type to use when creating transactions.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        Category = AttributeCategory.None,
        Order = 3 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts to display. By default all active accounts with a Public Name will be displayed. If the account has a child account for the selected campus, the child account for that campus will be used.",
        Category = AttributeCategory.None,
        Order = 5 )]

    [BooleanField(
        "Ask for Campus if Known",
        Key = AttributeKey.AskForCampusIfKnown,
        Description = "If the campus for the person is already known, should the campus still be prompted for?",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 10 )]

    [BooleanField(
        "Enable Multi-Account",
        Key = AttributeKey.EnableMultiAccount,
        Description = "Should the person be able specify amounts for more than one account?",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 11 )]

    [DefinedValueField(
        "Financial Source Type",
        Key = AttributeKey.FinancialSourceType,
        Description = "The Financial Source Type to use when creating transactions",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        Category = AttributeCategory.None,
        Order = 19 )]

    [BooleanField(
        "Enable Business Giving",
        Key = AttributeKey.EnableBusinessGiving,
        Description = "Should the option to give as a business be displayed.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.None,
        Order = 999 )]

    [BooleanField(
        "Enable Anonymous Giving",
        Key = AttributeKey.EnableAnonymousGiving,
        Description = "Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fund-raising contributors.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.None,
        Order = 24 )]

    [TextField(
        "Anonymous Giving Tool-tip",
        Key = AttributeKey.AnonymousGivingTooltip,
        Description = "The tool-tip for the 'Give Anonymously' check box.",
        Category = AttributeCategory.None,
        Order = 25 )]

    #region Scheduled Transactions

    [BooleanField(
        "Scheduled Transactions",
        Key = AttributeKey.AllowScheduledTransactions,
        Description = "If the selected gateway(s) allow scheduled transactions, should that option be provided to user.",
        TrueText = "Allow",
        FalseText = "Don't Allow",
        DefaultBooleanValue = true,
        Category = AttributeCategory.ScheduleGifts,
        Order = 1 )]

    [BooleanField(
        "Show Scheduled Gifts",
        Key = AttributeKey.ShowScheduledTransactions,
        Description = "If the person has any scheduled gifts, show a summary of their scheduled gifts.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.ScheduleGifts,
        Order = 2 )]

    [LinkedPage(
        "Scheduled Transaction Edit Page",
        Key = AttributeKey.ScheduledTransactionEditPage,
        Description = "The page to use for editing scheduled transactions.",
        Category = AttributeCategory.ScheduleGifts,
        Order = 3 )]

    #endregion

    #region Payment Comment Options

    [BooleanField(
        "Enable Comment Entry",
        Key = AttributeKey.EnableCommentEntry,
        Description = "Allows the guest to enter the value that's put into the comment field (will be appended to the 'Payment Comment' setting)",
        IsRequired = false,
        Category = AttributeCategory.PaymentComments,
        Order = 1 )]

    [TextField(
        "Comment Entry Label",
        Key = AttributeKey.EnableCommentEntry,
        Description = "The label to use on the comment edit field (e.g. Trip Name to give to a specific trip).",
        DefaultValue = "Comment",
        IsRequired = false,
        Category = AttributeCategory.PaymentComments,
        Order = 2 )]

    [CodeEditorField(
        "Payment Comment Template",
        Key = AttributeKey.PaymentCommentTemplate,
        Description = @"The comment to include with the payment transaction when sending to Gateway. <span class='tip tip-lava'></span>.",
        EditorMode = CodeEditorMode.Lava,
        Category = AttributeCategory.PaymentComments,
        Order = 3 )]

    #endregion Payment Comment Options

    #region Text Options

    [TextField( "Save Account Title",
        Key = AttributeKey.SaveAccountTitle,
        Description = "The text to display as heading of section for saving payment information.",
        IsRequired = false,
        DefaultValue = "Make Giving Even Easier",
        Category = AttributeCategory.TextOptions,
        Order = 1 )]

    [CodeEditorField(
        "Intro Message",
        Key = AttributeKey.IntroMessage,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text to place at the top of the amount entry",
        DefaultValue = "Your Generosity Changes Lives",
        Category = AttributeCategory.TextOptions,
        Order = 2 )]

    [TextField(
        "Gift Term",
        Key = AttributeKey.GiftTerm,
        DefaultValue = "Gift",
        Category = AttributeCategory.TextOptions,
        Order = 3 )]

    [TextField(
        "Give Button Text",
        Key = AttributeKey.GiveButtonText,
        DefaultValue = "Give Now",
        Category = AttributeCategory.TextOptions,
        Order = 4 )]

    [CodeEditorField(
        "Finish Lava Template",
        Key = AttributeKey.FinishLavaTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text (HTML) to display on the success page.",
        DefaultValue = DefaultFinishLavaTemplate,
        Category = AttributeCategory.TextOptions,
        Order = 5 )]

    [TextField(
        "Save Account Title",
        Key = AttributeKey.SaveAccountTitle,
        Description = "The text to display as heading of section for saving payment information.",
        DefaultValue = "Make Giving Even Easier",
        Category = AttributeCategory.TextOptions,
        Order = 6 )]

    #endregion

    #region Email Templates

    [SystemEmailField( "Confirm Account Email Template",
        Key = AttributeKey.ConfirmAccountEmailTemplate,
        Description = "The Email Template to use when confirming a new account",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT,
        Category = AttributeCategory.EmailTemplates,
        Order = 1 )]

    [SystemEmailField(
        "Receipt Email",
        Key = AttributeKey.ReceiptEmail,
        Description = "The system email to use to send the receipt.",
        IsRequired = false,
        Category = AttributeCategory.EmailTemplates,
        Order = 2 )]

    #endregion Email Templates

    #region Person Options

    [BooleanField(
        "Prompt for Phone",
        Key = AttributeKey.PromptForPhone,
        Category = AttributeCategory.PersonOptions,
        Description = "Should the user be prompted for their phone number?",
        DefaultBooleanValue = false,
        Order = 1 )]

    [BooleanField(
        "Prompt for Email",
        Key = AttributeKey.PromptForEmail,
        Category = AttributeCategory.PersonOptions,
        Description = "Should the user be prompted for their email address?",
        DefaultBooleanValue = true,
        Order = 2 )]

    [GroupLocationTypeField(
        "Address Type",
        Key = AttributeKey.PersonAddressType,
        Category = AttributeCategory.PersonOptions,
        Description = "The location type to use for the person's address",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        IsRequired = false,
        Order = 3 )]

    [DefinedValueField(
        "Connection Status",
        Key = AttributeKey.PersonConnectionStatus,
        Category = AttributeCategory.PersonOptions,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use for new individuals (default: 'Web Prospect'.)",
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT,
        IsRequired = true,
        Order = 4 )]

    [DefinedValueField(
        "Record Status",
        Key = AttributeKey.PersonRecordStatus,
        Category = AttributeCategory.PersonOptions,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Order = 5 )]

    #endregion Person Options

    #region Advanced options

    [DefinedValueField(
        "Transaction Type",
        Key = AttributeKey.TransactionType,
        Description = "",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        Category = AttributeCategory.Advanced,
        Order = 1 )]

    [EntityTypeField(
        "Transaction Entity Type",
        Key = AttributeKey.TransactionEntityType,
        Description = "The Entity Type for the Transaction Detail Record (usually left blank)",
        IsRequired = false,
        Category = AttributeCategory.Advanced,
        Order = 2 )]

    [TextField( "Entity Id Parameter",
        Key = AttributeKey.EntityIdParam,
        Description = "The Page Parameter that will be used to set the EntityId value for the Transaction Detail Record (requires Transaction Entry Type to be configured)",
        IsRequired = false,
        Category = AttributeCategory.Advanced,
        Order = 3 )]

    [AttributeField( "Allowed Transaction Attributes From URL",
        Key = AttributeKey.AllowedTransactionAttributesFromURL,
        EntityTypeGuid = Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION,
        Description = "Specify any Transaction Attributes that can be populated from the URL.  The URL should be formatted like: ?Attribute_AttributeKey1=hello&Attribute_AttributeKey2=world",
        IsRequired = false,
        AllowMultiple = true,
        Category = AttributeCategory.Advanced,
        Order = 4 )]

    [BooleanField(
        "Allow Account Options In URL",
        Key = AttributeKey.AllowAccountOptionsInURL,
        Description = "Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true",
        IsRequired = false,
        Category = AttributeCategory.Advanced,
        Order = 5 )]

    [BooleanField(
        "Only Public Accounts In URL",
        Key = AttributeKey.OnlyPublicAccountsInURL,
        Description = "Set to true if using the 'Allow Account Options In URL' option to prevent non-public accounts to be specified.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Advanced,
        Order = 6 )]

    [CodeEditorField(
        "Invalid Account Message",
        Key = AttributeKey.InvalidAccountMessage,
        Description = "Display this text (HTML) as an error alert if an invalid 'account' or 'GL account' is passed through the URL. Leave blank to just ignore the invalid accounts and not show a message.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "",
        Category = AttributeCategory.Advanced,
        Order = 7 )]

    [BooleanField( "Enable Initial Back button",
        Key = AttributeKey.EnableInitialBackButton,
        Description = "Show a Back button on the initial page that will navigate to wherever the user was prior to the transaction entry",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 8 )]

    [BooleanField(
        "Impersonation",
        Key = AttributeKey.AllowImpersonation,
        Description = "Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Advanced,
        Order = 9 )]

    #endregion Advanced Options

    #endregion Block Attributes
    public partial class TransactionEntryV2 : RockBlock
    {
        #region constants

        public const string DefaultFinishLavaTemplate = @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

<h1>Thank You!</h1>

<p>Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively achieve our
mission. We are so grateful for your commitment.</p>

<dl>
    <dt>Confirmation Code</dt>
    <dd>{{ Transaction.TransactionCode }}</dd>
    <dd></dd>
    
    <dt>Name</dt>
    <dd>{{ Person.FullName }}</dd>
    <dd></dd>
    <dd>{{ Person.Email }}</dd>
    <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
</dl>

<dl class='dl-horizontal'>
    {% for transactionDetail in transactionDetails %}
        <dt>{{ transactionDetail.Account.PublicName }}</dt>
        <dd>{{ transactionDetail.Amount }}</dd>
    {% endfor %}
    <dd></dd>
    
    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked  }}</dd>
    {% endif %}

    <dt>When<dt>
    <dd>
    {% if PaymentSchedule %}
        {{ PaymentSchedule | ToString }}
    {% else %}
        Today
    {% endif %}
    </dd>
</dl>
";
        #endregion

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string AccountsToDisplay = "AccountsToDisplay";

            public const string AllowImpersonation = "AllowImpersonation";

            public const string AllowScheduledTransactions = "AllowScheduledTransactions";

            public const string BatchNamePrefix = "BatchNamePrefix";

            public const string FinancialGateway = "FinancialGateway";

            public const string EnableACH = "EnableACH";

            public const string EnableCommentEntry = "EnableCommentEntry";

            public const string CommentEntryLabel = "CommentEntryLabel";

            public const string EnableBusinessGiving = "EnableBusinessGiving";

            public const string EnableAnonymousGiving = "EnableAnonymousGiving";

            public const string AnonymousGivingTooltip = "AnonymousGivingTooltip";

            public const string PaymentCommentTemplate = "PaymentCommentTemplate";

            public const string EnableInitialBackButton = "EnableInitialBackButton";

            public const string FinancialSourceType = "FinancialSourceType";

            public const string ShowScheduledTransactions = "ShowScheduledTransactions";

            public const string ScheduledTransactionEditPage = "ScheduledTransactionEditPage";

            public const string GiftTerm = "GiftTerm";

            public const string GiveButtonText = "Give Button Text";

            public const string AskForCampusIfKnown = "AskForCampusIfKnown";

            public const string EnableMultiAccount = "EnableMultiAccount";

            public const string IntroMessage = "IntroMessage";

            public const string FinishLavaTemplate = "FinishLavaTemplate";

            public const string SaveAccountTitle = "SaveAccountTitle";

            public const string ConfirmAccountEmailTemplate = "ConfirmAccountEmailTemplate";

            public const string TransactionType = "Transaction Type";

            public const string TransactionEntityType = "TransactionEntityType";

            public const string EntityIdParam = "EntityIdParam";

            public const string AllowedTransactionAttributesFromURL = "AllowedTransactionAttributesFromURL";

            public const string AllowAccountOptionsInURL = "AllowAccountOptionsInURL";

            public const string OnlyPublicAccountsInURL = "OnlyPublicAccountsInURL";

            public const string InvalidAccountMessage = "InvalidAccountMessage";

            public const string ReceiptEmail = "ReceiptEmail";

            public const string PromptForPhone = "PromptForPhone";

            public const string PromptForEmail = "PromptForEmail";

            public const string PersonAddressType = "PersonAddressType";

            public const string PersonConnectionStatus = "PersonConnectionStatus";

            public const string PersonRecordStatus = "PersonRecordStatus";
        }

        #endregion Attribute Keys

        #region Attribute Categories

        public static class AttributeCategory
        {
            public const string None = "";
            public const string ScheduleGifts = "Scheduled Gifts";
            public const string PaymentComments = "Payment Comments";
            public const string TextOptions = "Text Options";
            public const string Advanced = "Advanced";
            public const string EmailTemplates = "Email Templates";
            public const string PersonOptions = "Person Options";
        }

        #endregion Attribute Categories

        #region PageParameterKeys

        public static class PageParameterKey
        {
            public const string Person = "Person";

            public const string AttributeKeyPrefix = "Attribute_";
        }

        #endregion

        #region enums

        /// <summary>
        /// 
        /// </summary>
        private enum EntryStep
        {
            /// <summary>
            /// prompt for amounts (step 1)
            /// </summary>
            PromptForAmounts,

            /// <summary>
            /// Get payment information (step 2)
            /// </summary>
            GetPaymentInfo,

            /// <summary>
            /// Get/Update personal information (step 3)
            /// </summary>
            GetPersonalInformation,

            /// <summary>
            /// The show transaction summary (step 4)
            /// </summary>
            ShowTransactionSummary
        }

        #endregion enums

        #region fields

        private Control _hostedPaymentInfoControl;

        /// <summary>
        /// use FinancialGateway instead
        /// </summary>
        private Rock.Model.FinancialGateway _financialGateway = null;

        /// <summary>
        /// Gets the financial gateway (model) that is configured for this block
        /// </summary>
        /// <returns></returns>
        private Rock.Model.FinancialGateway FinancialGateway
        {
            get
            {
                if ( _financialGateway == null )
                {
                    RockContext rockContext = new RockContext();
                    var financialGatewayGuid = this.GetAttributeValue( AttributeKey.FinancialGateway ).AsGuid();
                    _financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayGuid );
                }

                return _financialGateway;
            }
        }

        private IHostedGatewayComponent _financialGatewayComponent = null;

        /// <summary>
        /// Gets the financial gateway component that is configured for this block
        /// </summary>
        /// <returns></returns>
        private IHostedGatewayComponent FinancialGatewayComponent
        {
            get
            {
                if ( _financialGatewayComponent == null )
                {
                    var financialGateway = FinancialGateway;
                    if ( financialGateway != null )
                    {
                        _financialGatewayComponent = financialGateway.GetGatewayComponent() as IHostedGatewayComponent;
                    }
                }

                return _financialGatewayComponent;
            }
        }

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the host payment information submit JavaScript.
        /// </summary>
        /// <value>
        /// The host payment information submit script.
        /// </value>
        protected string HostPaymentInfoSubmitScript
        {
            get
            {
                return ViewState["HostPaymentInfoSubmitScript"] as string;
            }

            set
            {
                ViewState["HostPaymentInfoSubmitScript"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the payment transaction code.
        /// </summary>
        protected string TransactionCode
        {
            get { return ViewState["TransactionCode"] as string ?? string.Empty; }
            set { ViewState["TransactionCode"] = value; }
        }

        /// <summary>
        /// Gets or sets the Customer Token for a newly created customer token from the payment info control.
        /// NOTE: Lets encrypt this since we don't want the ViewState to have an un-encrypted customer token, even though ViewState is already encrypted.
        /// </summary>
        /// <value>
        /// The customer token (encrypted)
        /// </value>
        protected string CustomerTokenEncrypted
        {
            get { return ViewState["CustomerTokenEncrypted"] as string ?? string.Empty; }
            set { ViewState["CustomerTokenEncrypted"] = value; }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            bool enableACH = this.GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            if ( this.FinancialGatewayComponent != null && this.FinancialGateway != null )
            {
                _hostedPaymentInfoControl = this.FinancialGatewayComponent.GetHostedPaymentInfoControl( this.FinancialGateway, enableACH, "_hostedPaymentInfoControl" );
                phHostedPaymentControl.Controls.Add( _hostedPaymentInfoControl );
                this.HostPaymentInfoSubmitScript = this.FinancialGatewayComponent.GetHostPaymentInfoSubmitScript( this.FinancialGateway, _hostedPaymentInfoControl );
            }

            if ( _hostedPaymentInfoControl is IHostedGatewayPaymentControlTokenEvent )
            {
                ( _hostedPaymentInfoControl as IHostedGatewayPaymentControlTokenEvent ).TokenReceived += _hostedPaymentInfoControl_TokenReceived;
            }

            var rockContext = new RockContext();
            var selectableAccountIds = new FinancialAccountService( rockContext ).GetByGuids( this.GetAttributeValues( AttributeKey.AccountsToDisplay ).AsGuidList() ).Select( a => a.Id ).ToArray();

            caapPromptForAccountAmounts.SelectableAccountIds = selectableAccountIds;
            caapPromptForAccountAmounts.AskForCampusIfKnown = this.GetAttributeValue( AttributeKey.AskForCampusIfKnown ).AsBoolean();
        }

        /// <summary>
        /// Handles the TokenReceived event of the _hostedPaymentInfoControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _hostedPaymentInfoControl_TokenReceived( object sender, EventArgs e )
        {
            string errorMessage = null;
            string token = this.FinancialGatewayComponent.GetHostedPaymentInfoToken( this.FinancialGateway, _hostedPaymentInfoControl, out errorMessage );
            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                nbPaymentTokenError.Text = errorMessage;
                nbPaymentTokenError.Visible = true;
            }
            else
            {
                nbPaymentTokenError.Visible = false;
                btnGetPaymentInfoNext_Click( sender, e );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // Ensure that there is only one transaction processed by getting a unique guid when this block loads for the first time
                // This will ensure there are no (unintended) duplicate transactions
                hfTransactionGuid.Value = Guid.NewGuid().ToString();
                ShowDetails();
            }
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            if ( !LoadGatewayOptions() )
            {
                return;
            }

            SetInitialTargetPersonControls();

            lIntroMessage.Text = this.GetAttributeValue( AttributeKey.IntroMessage );

            pnlTransactionEntry.Visible = true;
            bool enableMultiAccount = this.GetAttributeValue( AttributeKey.EnableMultiAccount ).AsBoolean();
            if ( enableMultiAccount )
            {
                caapPromptForAccountAmounts.AmountEntryMode = CampusAccountAmountPicker.AccountAmountEntryMode.MultipleAccounts;
            }
            else
            {
                caapPromptForAccountAmounts.AmountEntryMode = CampusAccountAmountPicker.AccountAmountEntryMode.SingleAccount;
            }

            if ( this.GetAttributeValue( AttributeKey.ShowScheduledTransactions ).AsBoolean() )
            {
                lScheduledTransactionsTitle.Text = string.Format( "Scheduled {0}", ( this.GetAttributeValue( AttributeKey.GiftTerm ) ?? "Gift" ).Pluralize() );
                pnlScheduledTransactions.Visible = true;
                BindScheduledTransactions();
            }
            else
            {
                pnlScheduledTransactions.Visible = false;
            }

            tbEmailIndividual.Visible = GetAttributeValue( AttributeKey.PromptForEmail ).AsBoolean();
            tbEmailBusiness.Visible = GetAttributeValue( AttributeKey.PromptForEmail ).AsBoolean();
            pnbPhoneIndividual.Visible = GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean();
            pnbPhoneBusiness.Visible = GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean();

            UpdateGivingControlsForSelections();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // If block options where changed, reload the whole page since changing some of the options (Gateway ACH Control options ) requires a full page reload
            this.NavigateToCurrentPageReference();
        }

        #endregion

        #region Methods

        #endregion

        #region Gateway Help Related

        /// <summary>
        /// Handles the Click event of the btnGatewayConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGatewayConfigure_Click( object sender, EventArgs e )
        {
            // TODO
        }

        /// <summary>
        /// Handles the Click event of the btnGatewayLearnMore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGatewayLearnMore_Click( object sender, EventArgs e )
        {
            // TODO
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptInstalledGateways control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptInstalledGateways_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            IHostedGatewayComponent financialGatewayComponent = e.Item.DataItem as IHostedGatewayComponent;
            if ( financialGatewayComponent == null )
            {
                return;
            }

            var gatewayEntityType = EntityTypeCache.Get( financialGatewayComponent.TypeGuid );
            var gatewayEntityTypeType = gatewayEntityType.GetEntityType();

            HiddenField hfGatewayEntityTypeId = e.Item.FindControl( "hfGatewayEntityTypeId" ) as HiddenField;
            hfGatewayEntityTypeId.Value = gatewayEntityType.Id.ToString();

            Literal lGatewayName = e.Item.FindControl( "lGatewayName" ) as Literal;
            Literal lGatewayDescription = e.Item.FindControl( "lGatewayDescription" ) as Literal;

            lGatewayName.Text = Reflection.GetDisplayName( gatewayEntityTypeType );
            lGatewayDescription.Text = Reflection.GetDescription( gatewayEntityTypeType );

            HyperLink aGatewayConfigure = e.Item.FindControl( "aGatewayConfigure" ) as HyperLink;
            HyperLink aGatewayLearnMore = e.Item.FindControl( "aGatewayLearnMore" ) as HyperLink;
            aGatewayConfigure.Visible = financialGatewayComponent.ConfigureURL.IsNotNullOrWhiteSpace();
            aGatewayLearnMore.Visible = financialGatewayComponent.LearnMoreURL.IsNotNullOrWhiteSpace();

            aGatewayConfigure.NavigateUrl = financialGatewayComponent.ConfigureURL;
            aGatewayLearnMore.NavigateUrl = financialGatewayComponent.LearnMoreURL;
        }

        /// <summary>
        /// Loads and Validates the gateways, showing a message if the gateways aren't configured correctly
        /// </summary>
        private bool LoadGatewayOptions()
        {
            if ( this.FinancialGatewayComponent == null )
            {
                ShowGatewayHelp();
                return false;
            }
            else
            {
                HideGatewayHelp();
            }

            var testGatewayGuid = Rock.SystemGuid.EntityType.FINANCIAL_GATEWAY_TEST_GATEWAY.AsGuid();

            if ( this.FinancialGatewayComponent.TypeGuid == testGatewayGuid )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Testing", "You are using the Test Financial Gateway. No actual amounts will be charged to your card or bank account." );
            }
            else
            {
                HideConfigurationMessage();
            }

            var supportedFrequencies = _financialGatewayComponent.SupportedPaymentSchedules;
            foreach ( var supportedFrequency in supportedFrequencies )
            {
                ddlFrequency.Items.Add( new ListItem( supportedFrequency.Value, supportedFrequency.Id.ToString() ) );
            }

            // If gateway didn't specifically support one-time, add it anyway for immediate gifts
            var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
            if ( !supportedFrequencies.Where( f => f.Id == oneTimeFrequency.Id ).Any() )
            {
                ddlFrequency.Items.Insert( 0, new ListItem( oneTimeFrequency.Value, oneTimeFrequency.Id.ToString() ) );
            }

            ddlFrequency.SelectedValue = oneTimeFrequency.Id.ToString();
            dtpStartDate.SelectedDate = RockDateTime.Today;
            pnlScheduledTransaction.Visible = this.GetAttributeValue( AttributeKey.AllowScheduledTransactions ).AsBoolean();

            return true;
        }

        /// <summary>
        /// Shows the gateway help
        /// </summary>
        private void ShowGatewayHelp()
        {
            pnlGatewayHelp.Visible = true;
            pnlTransactionEntry.Visible = false;

            var hostedGatewayComponentList = Rock.Financial.GatewayContainer.Instance.Components
                .Select( a => a.Value.Value )
                .Where( a => a is IHostedGatewayComponent )
                .Select( a => a as IHostedGatewayComponent ).ToList();

            rptInstalledGateways.DataSource = hostedGatewayComponentList;
            rptInstalledGateways.DataBind();
        }

        /// <summary>
        /// Hides the gateway help.
        /// </summary>
        private void HideGatewayHelp()
        {
            pnlGatewayHelp.Visible = false;
        }

        /// <summary>
        /// Shows the configuration message.
        /// </summary>
        /// <param name="notificationBoxType">Type of the notification box.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowConfigurationMessage( NotificationBoxType notificationBoxType, string title, string message )
        {
            nbConfigurationNotification.NotificationBoxType = notificationBoxType;
            nbConfigurationNotification.Title = title;
            nbConfigurationNotification.Text = message;

            nbConfigurationNotification.Visible = true;
        }

        /// <summary>
        /// Hides the configuration message.
        /// </summary>
        private void HideConfigurationMessage()
        {
            nbConfigurationNotification.Visible = false;
        }

        #endregion Gateway Guide Related

        #region Scheduled Gifts Related

        /// <summary>
        /// Binds the scheduled transactions.
        /// </summary>
        private void BindScheduledTransactions()
        {
            var rockContext = new RockContext();
            var targetPerson = GetTargetPerson( rockContext );

            if ( targetPerson == null )
            {
                pnlScheduledTransactions.Visible = false;
                return;
            }

            FinancialScheduledTransactionService financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );

            // get business giving id
            var givingIdList = targetPerson.GetBusinesses( rockContext ).Select( g => g.GivingId ).ToList();

            var targetPersonGivingId = targetPerson.GivingId;
            givingIdList.Add( targetPersonGivingId );
            var scheduledTransactionList = financialScheduledTransactionService.Queryable()
                .Where( a => givingIdList.Contains( a.AuthorizedPersonAlias.Person.GivingId ) && a.IsActive == true )
                .AsNoTracking()
                .ToList();

            foreach ( var scheduledTransaction in scheduledTransactionList )
            {
                string errorMessage;
                financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessage );
            }

            rockContext.SaveChanges();

            pnlScheduledTransactions.Visible = scheduledTransactionList.Any();

            scheduledTransactionList = scheduledTransactionList.OrderByDescending( a => a.NextPaymentDate ).ToList();
            rptScheduledTransactions.DataSource = scheduledTransactionList;
            rptScheduledTransactions.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduledTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduledTransactions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            FinancialScheduledTransaction financialScheduledTransaction = e.Item.DataItem as FinancialScheduledTransaction;
            if ( financialScheduledTransaction == null )
            {
                return;
            }

            HiddenField hfScheduledTransactionId = e.Item.FindControl( "hfScheduledTransactionId" ) as HiddenField;
            Literal lScheduledTransactionTitle = e.Item.FindControl( "lScheduledTransactionTitle" ) as Literal;
            Literal lScheduledTransactionAmountTotal = e.Item.FindControl( "lScheduledTransactionAmountTotal" ) as Literal;
            hfScheduledTransactionId.Value = financialScheduledTransaction.Id.ToString();
            lScheduledTransactionTitle.Text = financialScheduledTransaction.TransactionFrequencyValue.Value;
            lScheduledTransactionAmountTotal.Text = financialScheduledTransaction.TotalAmount.FormatAsCurrency();

            Repeater rptScheduledTransactionAccounts = e.Item.FindControl( "rptScheduledTransactionAccounts" ) as Repeater;
            rptScheduledTransactionAccounts.DataSource = financialScheduledTransaction.ScheduledTransactionDetails.ToList();
            rptScheduledTransactionAccounts.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduledTransactionAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduledTransactionAccounts_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            FinancialScheduledTransactionDetail financialScheduledTransactionDetail = e.Item.DataItem as FinancialScheduledTransactionDetail;
            Literal lScheduledTransactionAccountName = e.Item.FindControl( "lScheduledTransactionAccountName" ) as Literal;
            lScheduledTransactionAccountName.Text = financialScheduledTransactionDetail.Account.ToString();

            Literal lScheduledTransactionAmount = e.Item.FindControl( "lScheduledTransactionAmount" ) as Literal;
            lScheduledTransactionAmount.Text = financialScheduledTransactionDetail.Amount.FormatAsCurrency();
        }

        /// <summary>
        /// Handles the Click event of the btnScheduledTransactionEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnScheduledTransactionEdit_Click( object sender, EventArgs e )
        {
            var scheduledTransactionId = ( ( sender as LinkButton ).NamingContainer.FindControl( "hfScheduledTransactionId" ) as HiddenField ).Value.AsInteger();
            NavigateToLinkedPage( AttributeKey.ScheduledTransactionEditPage, "ScheduledTransactionId", scheduledTransactionId );
        }

        /// <summary>
        /// Handles the Click event of the btnScheduledTransactionDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnScheduledTransactionDelete_Click( object sender, EventArgs e )
        {
            var namingContainer = ( sender as LinkButton ).NamingContainer;
            var scheduledTransactionId = ( namingContainer.FindControl( "hfScheduledTransactionId" ) as HiddenField ).Value.AsInteger();
            NotificationBox nbScheduledTransactionMessage = namingContainer.FindControl( "nbScheduledTransactionMessage" ) as NotificationBox;
            Panel pnlActions = namingContainer.FindControl( "pnlActions" ) as Panel;

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                FinancialScheduledTransactionService financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                var scheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionId );
                if ( scheduledTransaction == null )
                {
                    return;
                }

                scheduledTransaction.FinancialGateway.LoadAttributes( rockContext );

                string errorMessage = string.Empty;
                if ( financialScheduledTransactionService.Cancel( scheduledTransaction, out errorMessage ) )
                {
                    try
                    {
                        financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessage );
                    }
                    catch
                    {
                        // ignore
                    }

                    rockContext.SaveChanges();

                    nbConfigurationNotification.Dismissable = true;
                    nbConfigurationNotification.NotificationBoxType = NotificationBoxType.Success;
                    nbScheduledTransactionMessage.Text = string.Format( "Your scheduled {0} has been deleted", GetAttributeValue( AttributeKey.GiftTerm ).ToLower() );
                    nbScheduledTransactionMessage.Visible = true;
                    pnlActions.Enabled = false;
                    pnlActions.Controls.OfType<LinkButton>().ToList().ForEach( a => a.Enabled = false );
                }
                else
                {
                    nbConfigurationNotification.Dismissable = true;
                    nbConfigurationNotification.NotificationBoxType = NotificationBoxType.Danger;
                    nbScheduledTransactionMessage.Text = string.Format( "An error occurred while deleting your scheduled {0}", GetAttributeValue( AttributeKey.GiftTerm ).ToLower() );
                    nbConfigurationNotification.Details = errorMessage;
                    nbScheduledTransactionMessage.Visible = true;
                    pnlActions.Enabled = false;
                    pnlActions.Controls.OfType<LinkButton>().ToList().ForEach( a => a.Enabled = false );
                }
            }
        }

        #endregion Scheduled Gifts

        #region Transaction Entry Related

        /// <summary>
        /// Initializes the UI based on the initial target person.
        /// </summary>
        private void SetInitialTargetPersonControls()
        {
            // If impersonation is allowed, and a valid person key was used, set the target to that person
            Person targetPerson = null;

            if ( GetAttributeValue( AttributeKey.AllowImpersonation ).AsBoolean() )
            {
                string personKey = PageParameter( PageParameterKey.Person );
                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    var incrementKeyUsage = !this.IsPostBack;
                    var rockContext = new RockContext();
                    targetPerson = new PersonService( rockContext ).GetByImpersonationToken( personKey, incrementKeyUsage, this.PageCache.Id );

                    if ( targetPerson == null )
                    {
                        nbInvalidPersonWarning.Text = "Invalid or Expired Person Token specified";
                        nbInvalidPersonWarning.NotificationBoxType = NotificationBoxType.Danger;
                        nbInvalidPersonWarning.Visible = true;
                        return;
                    }
                }
            }

            if ( targetPerson == null )
            {
                targetPerson = CurrentPerson;
            }

            if ( targetPerson != null )
            {
                hfTargetPersonId.Value = targetPerson.Id.ToString();
            }
            else
            {
                hfTargetPersonId.Value = string.Empty;
            }

            SetAccountPickerCampus( targetPerson );

            pnlLoggedInNameDisplay.Visible = targetPerson != null;
            if ( targetPerson != null )
            {
                lCurrentPersonFullName.Text = targetPerson.FullName;
                tbFirstName.Text = targetPerson.FirstName;
                tbLastName.Text = targetPerson.LastName;
                tbEmailIndividual.Text = targetPerson.Email;
                var rockContext = new RockContext();
                var addressTypeGuid = GetAttributeValue( AttributeKey.PersonAddressType ).AsGuid();
                var addressTypeId = DefinedValueCache.GetId( addressTypeGuid );

                GroupLocation personGroupLocation = null;
                if ( addressTypeId.HasValue )
                {
                    personGroupLocation = new PersonService( rockContext ).GetFirstLocation( targetPerson.Id, addressTypeId.Value );
                }

                if ( personGroupLocation != null )
                {
                    acAddressIndividual.SetValues( personGroupLocation.Location );
                }
                else
                {
                    acAddressIndividual.SetValues( null );
                }

                if ( GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean() )
                {
                    var personPhoneNumber = targetPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );

                    // If person did not have a home phone number, read the cell phone number (which would then
                    // get saved as a home number also if they don't change it, which is OK ).
                    if ( personPhoneNumber == null || string.IsNullOrWhiteSpace( personPhoneNumber.Number ) || personPhoneNumber.IsUnlisted )
                    {
                        personPhoneNumber = targetPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    }
                }

                cblSelectBusiness.Items.Clear();

                var personService = new PersonService( rockContext );
                var businesses = personService.GetBusinesses( targetPerson.Id ).Select( a => new
                {
                    a.Id,
                    a.LastName
                } ).ToList();

                if ( businesses.Any() )
                {
                    foreach ( var business in businesses )
                    {
                        cblSelectBusiness.Items.Add( new ListItem( business.LastName, business.Id.ToString() ) );
                    }

                    cblSelectBusiness.Items.Add( new ListItem( "New Business", "" ) );

                    cblSelectBusiness.Visible = true;
                    cblSelectBusiness.SelectedIndex = 0;
                }
                else
                {
                    //// person is associated with any businesses (yet),
                    //// so don't present the 'select business' prompt since they would only have the option to create a new business.
                    cblSelectBusiness.Visible = false;
                }
            }

            pnlAnonymousNameEntry.Visible = targetPerson == null;

            // show a prompt for Business Contact on the pnlPersonInformationAsBusiness panel if we don't have a target person so that we can create a person to be associated with the new business
            pnlBusinessContactAnonymous.Visible = targetPerson == null;
        }

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person GetTargetPerson( RockContext rockContext )
        {
            int? targetPersonId = hfTargetPersonId.Value.AsIntegerOrNull();
            if ( targetPersonId == null )
            {
                return null;
            }

            var targetPerson = new PersonService( rockContext ).GetNoTracking( targetPersonId.Value );
            return targetPerson;
        }

        /// <summary>
        /// Creates the target person from the information collected (Name, Phone, Email, Address), or returns a matching person if they already exist.
        /// NOTE: Use <seealso cref="CreateBusiness"/> to creating a Business(Person) record
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private Person CreateTargetPerson()
        {
            string firstName = tbFirstName.Text;
            string lastName = tbLastName.Text;
            string email = tbEmailIndividual.Text;

            if ( firstName.IsNotNullOrWhiteSpace() && lastName.IsNotNullOrWhiteSpace() && email.IsNotNullOrWhiteSpace() )
            {
                var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, pnbPhoneIndividual.Number );
                var matchingPerson = new PersonService( new RockContext() ).FindPerson( personQuery, true );
                if ( matchingPerson != null )
                {
                    return matchingPerson;
                }
            }

            return _createPersonOrBusiness( false, firstName, lastName, email );
        }

        /// <summary>
        /// Creates the business contact person.
        /// </summary>
        /// <returns></returns>
        private Person CreateBusinessContactPerson()
        {
            string firstName = tbBusinessContactFirstName.Text;
            string lastName = tbBusinessContactLastName.Text;
            string email = tbBusinessContactEmail.Text;

            if ( firstName.IsNotNullOrWhiteSpace() && lastName.IsNotNullOrWhiteSpace() && email.IsNotNullOrWhiteSpace() )
            {
                var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, pnbPhoneIndividual.Number );
                var matchingPerson = new PersonService( new RockContext() ).FindPerson( personQuery, true );
                if ( matchingPerson != null )
                {
                    return matchingPerson;
                }
            }

            return _createPersonOrBusiness( false, firstName, lastName, email );
        }

        /// <summary>
        /// Creates a business (or returns an existing business if the person already has a business with the same business name)
        /// </summary>
        /// <returns></returns>
        private Person CreateBusiness( Person contactPerson )
        {
            var businessName = tbBusinessName.Text;
            // Try to find existing business for person that has the same name
            var personBusinesses = contactPerson.GetBusinesses()
                .Where( b => b.LastName == businessName )
                .ToList();

            if ( personBusinesses.Count() == 1 )
            {
                return personBusinesses.First();
            }

            string email = tbEmailBusiness.Text;

            var business = _createPersonOrBusiness( true, null, businessName, email );

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            personService.AddContactToBusiness( business.Id, contactPerson.Id );

            return business;
        }

        /// <summary>
        /// Creates the person or business.
        /// </summary>
        /// <param name="createBusiness">if set to <c>true</c> [create business].</param>
        /// <returns></returns>
        private Person _createPersonOrBusiness( bool createBusiness, string firstName, string lastName, string email )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            DefinedValueCache dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.PersonConnectionStatus ).AsGuid() );
            DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.PersonRecordStatus ).AsGuid() );

            // Create Person
            var newPersonOrBusiness = new Person();
            newPersonOrBusiness.FirstName = firstName;
            newPersonOrBusiness.LastName = lastName;

            newPersonOrBusiness.IsEmailActive = true;
            newPersonOrBusiness.EmailPreference = EmailPreference.EmailAllowed;
            if ( createBusiness )
            {
                newPersonOrBusiness.RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() );
            }
            else
            {
                newPersonOrBusiness.RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
            }

            if ( dvcConnectionStatus != null )
            {
                newPersonOrBusiness.ConnectionStatusValueId = dvcConnectionStatus.Id;
            }

            if ( dvcRecordStatus != null )
            {
                newPersonOrBusiness.RecordStatusValueId = dvcRecordStatus.Id;
            }

            // Create Person and Family
            Group familyGroup = PersonService.SaveNewPerson( newPersonOrBusiness, rockContext, null, false );

            // SaveNewPerson should have already done this, but just in case
            rockContext.SaveChanges();

            return newPersonOrBusiness;
        }

        /// <summary>
        /// Updates the business from the information collected (Phone, Email, Address) and saves changes (if any) to the database.
        /// </summary>
        /// <param name="business">The business.</param>
        private void UpdateBusinessFromInputInformation( Person business )
        {
            _updatePersonOrBusinessFromInputInformation( business, true );
        }

        /// <summary>
        /// Updates the person from input information collected (Phone, Email, Address) and saves changes (if any) to the database..
        /// </summary>
        /// <param name="person">The person.</param>
        private void UpdatePersonFromInputInformation( Person person )
        {
            _updatePersonOrBusinessFromInputInformation( person, false );
        }

        /// <summary>
        /// Updates the person/business from the information collected (Phone, Email, Address) and saves changes (if any) to the database.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="paymentInfo">The payment information.</param>
        private void _updatePersonOrBusinessFromInputInformation( Person personOrBusiness, bool updateFromBusinessSelection )
        {
            var promptForEmail = this.GetAttributeValue( AttributeKey.PromptForEmail ).AsBoolean();
            var promptForPhone = this.GetAttributeValue( AttributeKey.PromptForPhone ).AsBoolean();
            PhoneNumberBox pnbPhone;
            EmailBox tbEmail;
            int numberTypeId;
            Guid locationTypeGuid;
            AddressControl acAddress;

            if ( updateFromBusinessSelection )
            {
                //personOrBusiness.LastName = tbBusinessName.Text;
                tbEmail = tbEmailBusiness;
                pnbPhone = pnbPhoneBusiness;
                numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;
                locationTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid();
                acAddress = acAddressBusiness;
            }
            else
            {
                //personOrBusiness.FirstName = tbFirstName.Text;
                //personOrBusiness.LastName = tbLastName.Text;

                tbEmail = tbEmailIndividual;
                pnbPhone = pnbPhoneIndividual;
                numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;
                locationTypeGuid = GetAttributeValue( AttributeKey.PersonAddressType ).AsGuid();
                acAddress = acAddressIndividual;
            }

            if ( promptForEmail )
            {
                personOrBusiness.Email = tbEmail.Text;
            }

            if ( promptForPhone )
            {
                if ( pnbPhone.Number.IsNotNullOrWhiteSpace() )
                {
                    var phone = personOrBusiness.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberTypeId );
                    if ( phone == null )
                    {
                        phone = new PhoneNumber();
                        personOrBusiness.PhoneNumbers.Add( phone );
                        phone.NumberTypeValueId = numberTypeId;
                    }

                    // TODO, verify if an unlisted home phone could get overwritten by their cell phone number if the home phone is unlisted
                    phone.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                    phone.Number = PhoneNumber.CleanNumber( pnbPhone.Number );
                }
            }

            var primaryFamily = personOrBusiness.GetFamily();

            if ( primaryFamily != null )
            {
                var rockContext = new RockContext();

                GroupService.AddNewGroupAddress(
                    rockContext,
                    primaryFamily,
                    locationTypeGuid.ToString(),
                    acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country,
                    true );
            }
        }

        /// <summary>
        /// Binds the person saved accounts that are available for the <paramref name="selectedScheduleFrequencyId"/>
        /// </summary>
        /// <param name="selectedScheduleFrequencyId">The selected schedule frequency identifier.</param>
        private void BindPersonSavedAccounts( int selectedScheduleFrequencyId )
        {
            ddlPersonSavedAccount.Visible = false;
            var currentSavedAccountSelection = ddlPersonSavedAccount.SelectedValue;

            int? targetPersonId = hfTargetPersonId.Value.AsIntegerOrNull();
            if ( targetPersonId == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var personSavedAccountsQuery = new FinancialPersonSavedAccountService( rockContext )
                .GetByPersonId( targetPersonId.Value )
                .Where( a => !a.IsSystem )
                .AsNoTracking();

            DefinedValueCache[] allowedCurrencyTypes = {
                DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid()),
                DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid())
                };

            int[] allowedCurrencyTypeIds = allowedCurrencyTypes.Select( a => a.Id ).ToArray();

            var financialGateway = this.FinancialGateway;
            if ( financialGateway == null )
            {
                return;
            }

            var financialGatewayComponent = this.FinancialGatewayComponent;
            if ( financialGatewayComponent == null )
            {
                return;
            }

            int oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0;
            bool oneTime = selectedScheduleFrequencyId == oneTimeFrequencyId;

            personSavedAccountsQuery = personSavedAccountsQuery.Where( a =>
                a.FinancialGatewayId == financialGateway.Id
                && ( a.FinancialPaymentDetail.CurrencyTypeValueId != null )
                && allowedCurrencyTypeIds.Contains( a.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );

            var personSavedAccountList = personSavedAccountsQuery.OrderBy( a => a.Name ).AsNoTracking().Select( a => new
            {
                a.Id,
                a.Name,
                a.FinancialPaymentDetail.AccountNumberMasked,
            } ).ToList();

            // Only show the SavedAccount picker if there are saved accounts. If there aren't any (or if they choose 'Use a different payment method'), a later step will prompt them to enter Payment Info (CC/ACH fields)
            ddlPersonSavedAccount.Visible = personSavedAccountList.Any();

            ddlPersonSavedAccount.Items.Clear();
            foreach ( var personSavedAccount in personSavedAccountList )
            {
                var displayName = string.Format( "{0} ({1})", personSavedAccount.Name, personSavedAccount.AccountNumberMasked );
                ddlPersonSavedAccount.Items.Add( new ListItem( displayName, personSavedAccount.Id.ToString() ) );
            }

            ddlPersonSavedAccount.Items.Add( new ListItem( "Use a different payment method", 0.ToString() ) );

            if ( currentSavedAccountSelection.IsNotNullOrWhiteSpace() )
            {
                ddlPersonSavedAccount.SetValue( currentSavedAccountSelection );
            }
            else
            {
                ddlPersonSavedAccount.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Sets the account picker campus from person
        /// </summary>
        private void SetAccountPickerCampus( Person person )
        {
            int? defaultCampusId = null;

            if ( person != null )
            {
                var personCampus = person.GetCampus();
                if ( personCampus != null )
                {
                    defaultCampusId = personCampus.Id;
                }
            }

            caapPromptForAccountAmounts.CampusId = defaultCampusId;
        }

        /// <summary>
        /// Determines if a Person's Saved Account was used as the payment method
        /// </summary>
        /// <returns></returns>
        private bool UsingPersonSavedAccount()
        {
            return ddlPersonSavedAccount.SelectedValue.AsInteger() > 0;
        }

        /// <summary>
        /// Navigates to step.
        /// </summary>
        /// <param name="entryStep">The entry step.</param>
        private void NavigateToStep( EntryStep entryStep )
        {
            pnlPromptForAmounts.Visible = entryStep == EntryStep.PromptForAmounts;

            pnlAmountSummary.Visible = entryStep == EntryStep.GetPersonalInformation
                || entryStep == EntryStep.GetPaymentInfo;

            pnlPersonalInformation.Visible = entryStep == EntryStep.GetPersonalInformation;
            pnlPaymentInfo.Visible = entryStep == EntryStep.GetPaymentInfo;
            pnlTransactionSummary.Visible = entryStep == EntryStep.ShowTransactionSummary;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ddlFrequency control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void ddlFrequency_SelectionChanged( object sender, EventArgs e )
        {
            UpdateGivingControlsForSelections();
        }

        /// <summary>
        /// Updates the giving controls based on what options are selected in the UI
        /// </summary>
        private void UpdateGivingControlsForSelections()
        {
            BindPersonSavedAccounts( ddlFrequency.SelectedValue.AsInteger() );

            int selectedScheduleFrequencyId = ddlFrequency.SelectedValue.AsInteger();

            int oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0;
            bool oneTime = selectedScheduleFrequencyId == oneTimeFrequencyId;
            var giftTerm = this.GetAttributeValue( AttributeKey.GiftTerm );

            if ( oneTime )
            {
                dtpStartDate.Label = string.Format( "Process {0} On", giftTerm );
            }
            else
            {
                dtpStartDate.Label = "Start Giving On";
            }

            // if scheduling recurring, it can't start today since the gateway will be taking care of automated giving, it might have already processed today's transaction. So make sure it is no earlier than tomorrow.
            if ( !oneTime && ( !dtpStartDate.SelectedDate.HasValue || dtpStartDate.SelectedDate.Value.Date <= RockDateTime.Today ) )
            {
                dtpStartDate.SelectedDate = RockDateTime.Today.AddDays( 1 );
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ddlPersonSavedAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPersonSavedAccount_SelectionChanged( object sender, EventArgs e )
        {
            UpdateGivingControlsForSelections();
        }

        #endregion Transaction Entry Related

        #region Navigation

        /// <summary>
        /// Handles the Click event of the btnGiveNow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGiveNow_Click( object sender, EventArgs e )
        {
            if ( caapPromptForAccountAmounts.IsValidAmountSelected() )
            {
                nbPromptForAmountsWarning.Visible = false;
                pnlPersonalInformation.Visible = false;
                var totalAmount = caapPromptForAccountAmounts.AccountAmounts.Sum( a => a.Amount ?? 0.00M );

                // get the accountId(s) that have an amount specified
                var amountAccountIds = caapPromptForAccountAmounts.AccountAmounts
                    .Where( a => a.Amount.HasValue && a.Amount != 0.00M ).Select( a => a.AccountId )
                    .ToList();

                var accountNames = new FinancialAccountService( new RockContext() )
                    .GetByIds( amountAccountIds )
                    .Select( a => a.PublicName )
                    .ToList().AsDelimited( ", ", " and " );

                lAmountSummaryAccounts.Text = accountNames;
                lAmountSummaryAmount.Text = totalAmount.FormatAsCurrency();
                if ( caapPromptForAccountAmounts.CampusId.HasValue )
                {
                    lAmountSummaryCampus.Text = CampusCache.Get( caapPromptForAccountAmounts.CampusId.Value ).Name;
                }

                if ( UsingPersonSavedAccount() )
                {
                    NavigateToStep( EntryStep.GetPersonalInformation );
                }
                else
                {
                    NavigateToStep( EntryStep.GetPaymentInfo );
                }
            }
            else
            {
                nbPromptForAmountsWarning.Visible = true;
                nbPromptForAmountsWarning.Text = "Please specify an amount";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnGetPaymentInfoBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGetPaymentInfoBack_Click( object sender, EventArgs e )
        {
            NavigateToStep( EntryStep.PromptForAmounts );
        }

        /// <summary>
        /// Handles the Click event of the btnGetPaymentInfoNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGetPaymentInfoNext_Click( object sender, EventArgs e )
        {
            //// NOTE: the btnGetPaymentInfoNext button tells _hostedPaymentInfoControl to get a token via JavaScript
            //// When _hostedPaymentInfoControl gets a token response, the _hostedPaymentInfoControl_TokenReceived event will be triggered
            //// If _hostedPaymentInfoControl_TokenReceived gets a valid token, it will call btnGetPaymentInfoNext_Click

            nbProcessTransactionError.Visible = false;
            NavigateToStep( EntryStep.GetPersonalInformation );
        }

        /// <summary>
        /// Handles the Click event of the btnPersonalInformationBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPersonalInformationBack_Click( object sender, EventArgs e )
        {
            if ( UsingPersonSavedAccount() )
            {
                NavigateToStep( EntryStep.PromptForAmounts );
            }
            else
            {
                NavigateToStep( EntryStep.GetPaymentInfo );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPersonalInformationNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPersonalInformationNext_Click( object sender, EventArgs e )
        {
            ProcessTransaction();
        }

        #endregion navigation

        /// <summary>
        /// Processes the transaction.
        /// </summary>
        /// <returns></returns>
        protected void ProcessTransaction()
        {
            var transactionGuid = hfTransactionGuid.Value.AsGuid();
            var rockContext = new RockContext();

            // to make duplicate transactions impossible, make sure that our Transaction hasn't already been processed as a regular or scheduled transaction
            bool transactionAlreadyExists = new FinancialTransactionService( rockContext ).Queryable().Any( a => a.Guid == transactionGuid );
            if ( !transactionAlreadyExists )
            {
                transactionAlreadyExists = new FinancialScheduledTransactionService( rockContext ).Queryable().Any( a => a.Guid == transactionGuid );
            }

            if ( transactionAlreadyExists )
            {
                ShowTransactionSummary();
            }


            bool givingAsBusiness = this.GivingAsBusiness();
            var financialGatewayComponent = this.FinancialGatewayComponent;
            string errorMessage;
            var paymentInfo = CreatePaymentInfoFromControls( givingAsBusiness );
            nbProcessTransactionError.Visible = false;

            // use the paymentToken as the reference number for creating the customer account
            var savedAccountId = ddlPersonSavedAccount.SelectedValue.AsIntegerOrNull();
            if ( savedAccountId.HasValue && savedAccountId.Value > 0 )
            {
                FinancialPersonSavedAccount financialPersonSavedAccount = new FinancialPersonSavedAccountService( rockContext ).Get( savedAccountId.Value );

                if ( financialPersonSavedAccount != null && financialPersonSavedAccount.ReferenceNumber.IsNotNullOrWhiteSpace() )
                {
                    paymentInfo.GatewayPersonIdentifier = financialPersonSavedAccount.ReferenceNumber;
                }
            }

            if ( paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
            {
                var paymentToken = financialGatewayComponent.GetHostedPaymentInfoToken( this.FinancialGateway, _hostedPaymentInfoControl, out errorMessage );
                var customerToken = financialGatewayComponent.CreateCustomerAccount( this.FinancialGateway, paymentToken, paymentInfo, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    nbProcessTransactionError.Text = errorMessage ?? "Unknown Error";
                    nbProcessTransactionError.Visible = true;
                    return;
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;

                // save the customer token in view state since we'll need it in case they create a saved account
                this.CustomerTokenEncrypted = Rock.Security.Encryption.EncryptString( customerToken );

            }

            // determine or create the Person record that this transaction is for
            var targetPerson = this.GetTargetPerson( rockContext );
            int transactionPersonId;

            if ( targetPerson == null )
            {
                if ( givingAsBusiness )
                {
                    targetPerson = this.CreateBusinessContactPerson();
                }
                else
                {
                    targetPerson = this.CreateTargetPerson();
                }

                hfTargetPersonId.Value = targetPerson.Id.ToString();
            }

            UpdatePersonFromInputInformation( targetPerson );

            if ( givingAsBusiness )
            {
                int? businessId = cblSelectBusiness.SelectedValue.AsInteger();
                var business = new PersonService( rockContext ).Get( businessId.Value );
                if ( business == null )
                {
                    business = CreateBusiness( targetPerson );
                }

                UpdateBusinessFromInputInformation( business );
                transactionPersonId = business.Id;
            }
            else
            {
                transactionPersonId = targetPerson.Id;
            }

            nbProcessTransactionError.Visible = false;

            if ( IsScheduledTransaction() )
            {

                string gatewayScheduleId = null;
                try
                {
                    PaymentSchedule paymentSchedule = new PaymentSchedule
                    {
                        TransactionFrequencyValue = DefinedValueCache.Get( ddlFrequency.SelectedValue.AsInteger() ),
                        StartDate = dtpStartDate.SelectedDate.Value,
                        PersonId = transactionPersonId
                    };

                    var financialScheduledTransaction = this.FinancialGatewayComponent.AddScheduledPayment( this.FinancialGateway, paymentSchedule, paymentInfo, out errorMessage );
                    if ( financialScheduledTransaction == null )
                    {
                        if ( errorMessage.IsNullOrWhiteSpace() )
                        {
                            errorMessage = "Unknown Error";
                        }

                        nbProcessTransactionError.Text = errorMessage;
                        nbProcessTransactionError.Visible = true;
                        return;
                    }

                    gatewayScheduleId = financialScheduledTransaction.GatewayScheduleId;

                    SaveScheduledTransaction( transactionPersonId, paymentInfo, paymentSchedule, financialScheduledTransaction );
                }
                catch ( Exception ex )
                {
                    if ( gatewayScheduleId.IsNotNullOrWhiteSpace() )
                    {
                        // if we didn't get the gatewayScheduleId from AddScheduledPayment, see if the gateway paymentInfo.TransactionCode before the exception occurred
                        gatewayScheduleId = paymentInfo.TransactionCode;
                    }

                    throw new Exception( string.Format( "Error occurred when saving financial scheduled transaction for gateway scheduled payment with a gatewayScheduleId of {0} and FinancialScheduledTransaction with Guid of {1}.", gatewayScheduleId, transactionGuid ), ex );
                }
            }
            else
            {
                string transactionCode = null;
                try
                {
                    FinancialTransaction financialTransaction = this.FinancialGatewayComponent.Charge( this.FinancialGateway, paymentInfo, out errorMessage );
                    if ( financialTransaction == null )
                    {
                        if ( errorMessage.IsNullOrWhiteSpace() )
                        {
                            errorMessage = "Unknown Error";
                        }

                        nbProcessTransactionError.Text = errorMessage;
                        nbProcessTransactionError.Visible = true;
                        return;
                    }

                    transactionCode = financialTransaction.TransactionCode;

                    SaveTransaction( transactionPersonId, paymentInfo, financialTransaction );
                }
                catch ( Exception ex )
                {
                    throw new Exception( string.Format( "Error occurred when saving financial transaction for gateway payment with a transactionCode of {0} and FinancialTransaction with Guid of {1}.", transactionCode, transactionGuid ), ex );
                }
            }

            ShowTransactionSummary();
        }

        /// <summary>
        /// Giving as business.
        /// </summary>
        /// <returns></returns>
        private bool GivingAsBusiness()
        {
            return tglIndividualOrBusiness.Checked;
        }

        /// <summary>
        /// Determines whether a scheduled giving frequency was selected
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is scheduled transaction]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsScheduledTransaction()
        {
            int oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0;
            return ddlFrequency.SelectedValue.AsInteger() != oneTimeFrequencyId;
        }

        /// <summary>
        /// Creates a PaymentInfo object from the information collected in the UI
        /// </summary>
        /// <param name="givingAsBusiness">if set to <c>true</c> [giving as business].</param>
        /// <returns></returns>
        private ReferencePaymentInfo CreatePaymentInfoFromControls( bool givingAsBusiness )
        {
            var acAddress = givingAsBusiness ? acAddressBusiness : acAddressIndividual;
            var tbEmail = givingAsBusiness ? tbEmailBusiness : tbEmailIndividual;
            var pnbPhone = givingAsBusiness ? pnbPhoneBusiness : pnbPhoneIndividual;

            var paymentInfo = new ReferencePaymentInfo
            {
                Street1 = acAddress.Street1,
                Street2 = acAddress.Street2,
                City = acAddress.City,
                State = acAddress.State,
                PostalCode = acAddress.PostalCode,
                Country = acAddress.Country,
                Email = tbEmail.Text,
                Phone = PhoneNumber.FormattedNumber( pnbPhone.CountryCode, pnbPhone.Number, true )
            };

            if ( givingAsBusiness )
            {
                if ( pnlBusinessContactAnonymous.Visible )
                {
                    paymentInfo.FirstName = tbBusinessContactFirstName.Text;
                    paymentInfo.LastName = tbBusinessContactLastName.Text;
                }
                else
                {
                    paymentInfo.FirstName = tbFirstName.Text;
                    paymentInfo.LastName = tbLastName.Text;
                }

                paymentInfo.BusinessName = tbBusinessName.Text;
            }
            else
            {
                paymentInfo.FirstName = tbFirstName.Text;
                paymentInfo.LastName = tbLastName.Text;
            }

            return paymentInfo;
        }

        /// <summary>
        /// Shows the transaction summary.
        /// </summary>
        /// <param name="financialTransaction">The financial transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        protected void ShowTransactionSummary()
        {
            var rockContext = new RockContext();
            var transactionGuid = hfTransactionGuid.Value.AsGuid();

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            var finishLavaTemplate = this.GetAttributeValue( AttributeKey.FinishLavaTemplate );

            // the transactionGuid is either for a FinancialTransaction or a FinancialScheduledTransaction
            int? financialPaymentDetailId;
            FinancialPaymentDetail financialPaymentDetail;
            FinancialTransaction financialTransaction = new FinancialTransactionService( rockContext ).Get( transactionGuid );
            if ( financialTransaction != null )
            {
                mergeFields.Add( "Transaction", financialTransaction );
                mergeFields.Add( "Person", financialTransaction.AuthorizedPersonAlias.Person );
                financialPaymentDetail = financialTransaction.FinancialPaymentDetail;
                financialPaymentDetailId = financialTransaction.FinancialGatewayId;
            }
            else
            {
                FinancialScheduledTransaction financialScheduledTransaction = new FinancialScheduledTransactionService( rockContext ).Get( transactionGuid );
                mergeFields.Add( "Transaction", financialScheduledTransaction );
                mergeFields.Add( "Person", financialScheduledTransaction.AuthorizedPersonAlias.Person );
                financialPaymentDetail = financialScheduledTransaction.FinancialPaymentDetail;
                financialPaymentDetailId = financialScheduledTransaction.FinancialGatewayId;
            }

            if ( financialPaymentDetail != null || financialPaymentDetailId.HasValue )
            {
                financialPaymentDetail = financialPaymentDetail ?? new FinancialPaymentDetailService( rockContext ).GetNoTracking( financialPaymentDetailId.Value );
                mergeFields.Add( "PaymentDetail", financialPaymentDetail );

                if ( financialPaymentDetail.BillingLocation != null || financialPaymentDetail.BillingLocationId.HasValue )
                {
                    var billingLocation = financialPaymentDetail.BillingLocation ?? new LocationService( rockContext ).GetNoTracking( financialPaymentDetail.BillingLocationId.Value );
                    mergeFields.Add( "BillingLocation", billingLocation );
                }
            }

            lTransactionSummaryHTML.Text = finishLavaTemplate.ResolveMergeFields( mergeFields );

            if ( !UsingPersonSavedAccount() )
            {
                pnlSaveAccountPrompt.Visible = true;

                // Show save account info based on if checkbox is checked
                pnlSaveAccountEntry.Style[HtmlTextWriterStyle.Display] = cbSaveAccount.Checked ? "block" : "none";
            }

            // If target person does not have a login, have them create a UserName and password
            var targetPerson = GetTargetPerson( rockContext );
            var hasUserLogin = new UserLoginService( rockContext ).GetByPersonId( targetPerson.Id ).Any();
            pnlCreateLogin.Visible = !hasUserLogin;

            NavigateToStep( EntryStep.ShowTransactionSummary );
        }

        #region cleanup

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="transaction">The transaction.</param>
        private void SaveTransaction( int personId, PaymentInfo paymentInfo, FinancialTransaction transaction )
        {
            FinancialGateway financialGateway = this.FinancialGateway;
            IHostedGatewayComponent gateway = this.FinancialGatewayComponent;
            var rockContext = new RockContext();

            // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
            transaction.Guid = hfTransactionGuid.Value.AsGuid();

            transaction.AuthorizedPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId );
            // TODO: transaction.ShowAsAnonymous = cbGiveAnonymously.Checked;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = financialGateway.Id;

            var txnType = DefinedValueCache.Get( this.GetAttributeValue( AttributeKey.TransactionType ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            transaction.TransactionTypeValueId = txnType.Id;

            transaction.Summary = paymentInfo.Comment1;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway as GatewayComponent, rockContext );

            Guid? sourceGuid = GetAttributeValue( AttributeKey.FinancialSourceType ).AsGuidOrNull();
            if ( sourceGuid.HasValue )
            {
                transaction.SourceTypeValueId = DefinedValueCache.GetId( sourceGuid.Value );
            }

            PopulateTransactionDetails( transaction.TransactionDetails );

            var batchService = new FinancialBatchService( rockContext );

            // Get the batch
            var batch = batchService.Get(
                GetAttributeValue( AttributeKey.BatchNamePrefix ),
                paymentInfo.CurrencyTypeValue,
                paymentInfo.CreditCardTypeValue,
                transaction.TransactionDateTime.Value,
                financialGateway.GetBatchTimeOffset() );

            var batchChanges = new History.HistoryChangeList();

            if ( batch.Id == 0 )
            {
                batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
            }

            decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
            History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
            batch.ControlAmount = newControlAmount;

            transaction.LoadAttributes( rockContext );

            var allowedTransactionAttributes = GetAttributeValue( AttributeKey.AllowedTransactionAttributesFromURL ).Split( ',' ).AsGuidList().Select( x => AttributeCache.Get( x ).Key );

            foreach ( KeyValuePair<string, AttributeValueCache> attr in transaction.AttributeValues )
            {
                if ( PageParameters().ContainsKey( PageParameterKey.AttributeKeyPrefix + attr.Key ) && allowedTransactionAttributes.Contains( attr.Key ) )
                {
                    attr.Value.Value = Server.UrlDecode( PageParameter( PageParameterKey.AttributeKeyPrefix + attr.Key ) );
                }
            }

            var financialTransactionService = new FinancialTransactionService( rockContext );

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if ( batch.Id == 0 )
            {
                rockContext.SaveChanges();
            }

            transaction.BatchId = batch.Id;

            // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
            financialTransactionService.Add( transaction );
            rockContext.SaveChanges();

            transaction.SaveAttributeValues();

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                batch.Id,
                batchChanges );

            SendReceipt( transaction.Id );

            TransactionCode = transaction.TransactionCode;
        }

        /// <summary>
        /// Populates the transaction details for a FinancialTransaction or ScheduledFinancialTransaction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transactionDetails">The transaction details.</param>
        private void PopulateTransactionDetails<T>( ICollection<T> transactionDetails ) where T : ITransactionDetail, new()
        {
            var transactionEntity = this.GetTransactionEntity();
            var selectedAccountAmounts = caapPromptForAccountAmounts.AccountAmounts;

            foreach ( var selectedAccountAmount in selectedAccountAmounts.Where( a => a.Amount.HasValue && a.Amount != 0 ) )
            {
                var transactionDetail = new T();
                transactionDetail.Amount = selectedAccountAmount.Amount.Value;
                transactionDetail.AccountId = selectedAccountAmount.AccountId;

                if ( transactionEntity != null )
                {
                    transactionDetail.EntityTypeId = transactionEntity.TypeId;
                    transactionDetail.EntityId = transactionEntity.Id;
                }

                transactionDetails.Add( transactionDetail );
            }
        }

        /// <summary>
        /// Saves the scheduled transaction.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        private void SaveScheduledTransaction( int personId, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialScheduledTransaction scheduledTransaction )
        {
            FinancialGateway financialGateway = this.FinancialGateway;
            IHostedGatewayComponent gateway = this.FinancialGatewayComponent;
            var rockContext = new RockContext();

            // manually assign the Guid that we generated at the beginning of the transaction UI entry to help make duplicate transactions impossible
            scheduledTransaction.Guid = hfTransactionGuid.Value.AsGuid();

            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
            scheduledTransaction.AuthorizedPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId ).Value;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;

            scheduledTransaction.Summary = paymentInfo.Comment1;

            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway as GatewayComponent, rockContext );

            Guid? sourceGuid = GetAttributeValue( AttributeKey.FinancialSourceType ).AsGuidOrNull();
            if ( sourceGuid.HasValue )
            {
                scheduledTransaction.SourceTypeValueId = DefinedValueCache.GetId( sourceGuid.Value );
            }

            PopulateTransactionDetails( scheduledTransaction.ScheduledTransactionDetails );

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            financialScheduledTransactionService.Add( scheduledTransaction );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the transaction entity.
        /// </summary>
        /// <returns></returns>
        private IEntity GetTransactionEntity()
        {
            IEntity transactionEntity = null;
            Guid? transactionEntityTypeGuid = GetAttributeValue( AttributeKey.TransactionEntityType ).AsGuidOrNull();
            if ( transactionEntityTypeGuid.HasValue )
            {
                var transactionEntityType = EntityTypeCache.Get( transactionEntityTypeGuid.Value );
                if ( transactionEntityType != null )
                {
                    var entityId = this.PageParameter( this.GetAttributeValue( AttributeKey.EntityIdParam ) ).AsIntegerOrNull();
                    if ( entityId.HasValue )
                    {
                        var dbContext = Reflection.GetDbContextForEntityType( transactionEntityType.GetEntityType() );
                        IService serviceInstance = Reflection.GetServiceForEntityType( transactionEntityType.GetEntityType(), dbContext );
                        if ( serviceInstance != null )
                        {
                            System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                            transactionEntity = getMethod.Invoke( serviceInstance, new object[] { entityId.Value } ) as Rock.Data.IEntity;
                        }
                    }
                }
            }

            return transactionEntity;
        }

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        private void SendReceipt( int transactionId )
        {
            Guid? receiptEmail = GetAttributeValue( AttributeKey.ReceiptEmail ).AsGuidOrNull();
            if ( receiptEmail.HasValue )
            {
                // Queue a transaction to send receipts
                var newTransactionIds = new List<int> { transactionId };
                var sendPaymentReceiptsTxn = new Rock.Transactions.SendPaymentReceipts( receiptEmail.Value, newTransactionIds );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendPaymentReceiptsTxn );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAccount_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var targetPerson = GetTargetPerson( rockContext );

            if ( pnlCreateLogin.Visible )
            {
                string errorTitle = null;
                string errorMessage = null;
                if ( !UserLoginService.IsValidNewUserLogin( tbUserName.Text, tbPassword.Text, tbPasswordConfirm.Text, out errorTitle, out errorMessage ) )
                {
                    nbSaveAccount.Title = errorTitle;
                    nbSaveAccount.Text = errorMessage;
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Validation;
                    nbSaveAccount.Visible = true;
                    return;
                }

                var userLogin = UserLoginService.Create(
                    rockContext,
                    targetPerson,
                    Rock.Model.AuthenticationServiceType.Internal,
                    EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                    tbUserName.Text,
                    tbPassword.Text,
                    false );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
                mergeFields.Add( "Person", targetPerson );
                mergeFields.Add( "User", userLogin );

                var emailMessage = new RockEmailMessage( GetAttributeValue( AttributeKey.ConfirmAccountEmailTemplate ).AsGuid() );
                emailMessage.AddRecipient( new RecipientData( targetPerson.Email, mergeFields ) );
                emailMessage.AppRoot = ResolveRockUrl( "~/" );
                emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                emailMessage.CreateCommunicationRecord = false;
                emailMessage.Send();
            }


            var financialGatewayComponent = this.FinancialGatewayComponent;
            var financialGateway = this.FinancialGateway;

            var financialTransaction = new FinancialTransactionService( rockContext ).Get( hfTransactionGuid.Value.AsGuid() );

            var gatewayPersonIdentifier = Rock.Security.Encryption.DecryptString( this.CustomerTokenEncrypted );

            var savedAccount = new FinancialPersonSavedAccount();
            var paymentDetail = financialTransaction.FinancialPaymentDetail;

            savedAccount.PersonAliasId = targetPerson.PrimaryAliasId;
            savedAccount.ReferenceNumber = gatewayPersonIdentifier;
            savedAccount.Name = tbSaveAccount.Text;
            savedAccount.TransactionCode = TransactionCode;
            savedAccount.FinancialGatewayId = financialGateway.Id;
            savedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
            savedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentDetail.AccountNumberMasked;
            savedAccount.FinancialPaymentDetail.CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId;
            savedAccount.FinancialPaymentDetail.CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId;
            savedAccount.FinancialPaymentDetail.NameOnCardEncrypted = paymentDetail.NameOnCardEncrypted;
            savedAccount.FinancialPaymentDetail.ExpirationMonthEncrypted = paymentDetail.ExpirationMonthEncrypted;
            savedAccount.FinancialPaymentDetail.ExpirationYearEncrypted = paymentDetail.ExpirationYearEncrypted;
            savedAccount.FinancialPaymentDetail.BillingLocationId = paymentDetail.BillingLocationId;

            var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
            savedAccountService.Add( savedAccount );
            rockContext.SaveChanges();

            cbSaveAccount.Visible = false;
            tbSaveAccount.Visible = false;
            pnlCreateLogin.Visible = false;
            divSaveActions.Visible = false;

            nbSaveAccount.Title = "Success";
            nbSaveAccount.Text = "The account has been saved for future use";
            nbSaveAccount.NotificationBoxType = NotificationBoxType.Success;
            nbSaveAccount.Visible = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblSelectBusiness control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblSelectBusiness_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdatePersonalInformationFromSelectedBusiness();
        }

        /// <summary>
        /// Updates the Personal/Business info when giving as a business
        /// </summary>
        private void UpdatePersonalInformationFromSelectedBusiness()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            int? selectedBusinessPersonId = cblSelectBusiness.SelectedValue.AsIntegerOrNull();
            Person personAsBusiness = null;
            if ( selectedBusinessPersonId.HasValue )
            {
                personAsBusiness = personService.Get( selectedBusinessPersonId.Value );
            }

            if ( personAsBusiness == null )
            {
                tbBusinessName.Text = null;
                acAddressBusiness.SetValues( null );
                tbEmailBusiness.Text = string.Empty;
                pnbPhoneBusiness.Text = string.Empty;
            }
            else
            {
                tbBusinessName.Text = personAsBusiness.LastName;

                Guid addressTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid();
                var addressTypeId = DefinedValueCache.GetId( addressTypeGuid );

                GroupLocation businessLocation = null;
                if ( addressTypeId.HasValue )
                {
                    businessLocation = new PersonService( rockContext ).GetFirstLocation( personAsBusiness.Id, addressTypeId.Value );
                }

                if ( businessLocation != null )
                {
                    acAddressBusiness.SetValues( businessLocation.Location );
                }
                else
                {
                    acAddressBusiness.SetValues( null );
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglIndividualOrBusiness control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglIndividualOrBusiness_CheckedChanged( object sender, EventArgs e )
        {
            bool givingAsBusiness = GivingAsBusiness();
            pnlPersonInformationAsIndividual.Visible = !givingAsBusiness;
            pnlPersonInformationAsBusiness.Visible = givingAsBusiness;
            UpdatePersonalInformationFromSelectedBusiness();
        }
    }

    #endregion cleanup
}