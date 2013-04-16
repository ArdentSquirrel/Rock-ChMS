﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>    
    [CustomCheckboxListField( "Credit Card Provider", "Which payment processor should be used for credit cards?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialGateway]", true, "", "Payments", 1 )]
    [CustomCheckboxListField( "Checking/ACH Provider", "Which payment processor should be used for checking/ACH?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialGateway]", true, "", "Payments", 2 )]
    [CustomCheckboxListField( "Default Accounts to display", "Which accounts should be displayed by default?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [FinancialAccount] WHERE [IsActive] = 1 ORDER BY [Order]", true, "", "Payments", 3 )]
    [BooleanField( "Stack layout vertically", "Should giving UI be stacked vertically or horizontally?", true, "UI Options", 2 )]
    [BooleanField( "Show Campus selection", "Should giving be associated with a specific campus?", false, "UI Options", 3 )]
    [BooleanField( "Show Credit Card giving", "Allow users to give using a credit card?", true, "UI Options", 4 )]
    [BooleanField( "Show Checking/ACH giving", "Allow users to give using a checking account?", true, "UI Options", 5 )]
    public partial class RecurringGift : RockBlock
    {
        #region Fields

        protected bool _UseStackedLayout = false;
        protected bool _ShowCampusSelect = false;
        protected bool _ShowSaveDetails = false;
        protected bool _ShowCreditCard = false;
        protected bool _ShowChecking = false;
        protected string spanClass = "";

        protected List<FinancialTransactionDetail> _detailList = new List<FinancialTransactionDetail>();
        protected FinancialTransactionService _transactionService = new FinancialTransactionService();
        protected FinancialTransaction _transaction = new FinancialTransaction();

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _UseStackedLayout = Convert.ToBoolean( GetAttributeValue( "Stacklayoutvertically" ) );
            _ShowCampusSelect = Convert.ToBoolean( GetAttributeValue( "ShowCampusselection" ) );
            _ShowCreditCard = Convert.ToBoolean( GetAttributeValue( "ShowCreditCardgiving" ) );
            _ShowChecking = Convert.ToBoolean( GetAttributeValue( "ShowChecking/ACHgiving" ) );

            if ( CurrentPerson != null )
            {
                _ShowSaveDetails = true;
            }

            if ( _ShowCampusSelect )
            {
                BindCampuses();
            }

            if ( !IsPostBack )
            {
                BindAccounts();
                BindOptions();
                BindPersonDetails();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            base.OnLoad( e );
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnAddAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddAccount_SelectionChanged( object sender, EventArgs e )
        {
            FinancialAccountService accountService = new FinancialAccountService(); 
            FinancialTransactionDetail account;
            _detailList.Clear();
            
            var lookupAccounts = accountService.Queryable().Where(f => f.IsActive)
                .Distinct().OrderBy(f => f.Order).ToList();

            foreach ( RepeaterItem item in rptAccountList.Items )
            {
                account = new FinancialTransactionDetail();                
                
                // TODO rewrite account lookup to use ID instead of name
                string accountName = ( (HtmlGenericControl)item.FindControl("lblAccountName") ).InnerText;
                account.Account = lookupAccounts.Where(f => f.PublicName == accountName).FirstOrDefault();
                decimal amount = Decimal.Parse(( (HtmlInputControl)item.FindControl("inputAccountAmount") ).Value);                
                account.Amount = amount;                
                account.TransactionId = _transaction.Id;
                
                _detailList.Add(account);
            }
            
            account = new FinancialTransactionDetail();
            account.Account = lookupAccounts.Where( f => f.PublicName == btnAddAccount.SelectedValue ).FirstOrDefault();
            account.Amount = 0M;
            _detailList.Add( account );

            if ( btnAddAccount.Items.Count > 1 )
            {
                btnAddAccount.Items.Remove( btnAddAccount.SelectedValue );
                btnAddAccount.Title = "Add Another Gift";
            }
            else
            {
                divAddAccount.Visible = false;
            }

            rptAccountList.DataSource = _detailList.ToDictionary(f => (string)f.Account.PublicName, f => (decimal)f.Amount);
            rptAccountList.DataBind();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the btnRecurrence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRecurrence_SelectionChanged( object sender, EventArgs e )
        {
            divRecurrence.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {
            pnlConfirm.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            FinancialTransactionDetailService detailService = new FinancialTransactionDetailService();
            PersonService personService = new PersonService();
            FinancialAccountService accountService = new FinancialAccountService();            
            _transactionService = new FinancialTransactionService();            
            
            Person person;

            // process person details          
            var personGroup = personService.GetByEmail(Request.Form["txtEmail"]);

            if ( personGroup.Count() > 0 )
            {
                person = personGroup.Where(p => p.FirstName == Request.Form["txtFirstName"]
                    && p.LastName == Request.Form["txtLastName"]).Distinct().FirstOrDefault();
                // TODO duplicate person handling?  ex NewAccount.ascx DisplayDuplicates()
            }
            else
            {
                person = new Person();
                personService.Add(person, CurrentPersonId);
            }

            person.Email = txtEmail.Value;
            person.GivenName = txtFirstName.Value;
            person.LastName = txtLastName.Value;

            personService.Save(person, CurrentPersonId);
            cfrmName.Text = person.FullName;

            // process gift details
            var lookupAccounts = accountService.Queryable().Where(f => f.IsActive)
                .Distinct().OrderBy(f => f.Order).ToList();

            _transaction = (FinancialTransaction)ViewState["CachedTransaction"];
            if ( _transaction == null )
            {
                _transaction = new FinancialTransaction();
                _transaction.TransactionTypeValueId = Rock.Web.Cache.DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ).Id;
                _transactionService.Add( _transaction, person.Id );
            }

            foreach ( RepeaterItem item in rptAccountList.Items )
            {
                FinancialTransactionDetail account = new FinancialTransactionDetail();
                FinancialTransactionDetail detail = new FinancialTransactionDetail();

                // TODO rewrite account lookup to use ID instead of name
                string accountName = ( (HtmlGenericControl)item.FindControl("lblAccountName") ).InnerText;
                account.Account = lookupAccounts.Where(f => f.PublicName == accountName).FirstOrDefault();
                decimal amount = Decimal.Parse(( (HtmlInputControl)item.FindControl("inputAccountAmount") ).Value);
                detail.Amount = amount;
                account.Amount = amount;
                detail.TransactionId = _transaction.Id;
                account.TransactionId = _transaction.Id;
                detail.Summary = "$" + amount + " contribution to " + account.Account + " by " + person.FullName;

                detailService.Add(detail, person.Id);
                _detailList.Add(account);
            }

            _transaction.AuthorizedPersonId = person.Id;
            _transaction.Amount = _detailList.Sum(g => (decimal)g.Amount);
            ViewState["CachedTransaction"] = _transaction;
            _transactionService.Save(_transaction, CurrentPersonId);

            // process payment type
            if ( !string.IsNullOrEmpty(hfCardType.Value) )
            {
                litPaymentType.Text = hfCardType.Value.Split(' ').Reverse().FirstOrDefault().Substring(3);
                litPaymentType.Text = char.ToUpper(litPaymentType.Text[0]) + litPaymentType.Text.Substring(1);
                litAccountType.Text = " credit card ";
            }
            else
            {
                litPaymentType.Text = radioAccountType.SelectedValue;
                litAccountType.Text = " account ";
            }

            string lastFour = !string.IsNullOrEmpty(numCreditCard.Value)
                ? numCreditCard.Value
                : numAccount.Value;

            if ( !string.IsNullOrEmpty(lastFour) )
            {
                lblPaymentLastFour.Text = lastFour.Substring(lastFour.Length - 4, 4);
            }
            else
            {
                divPaymentConfirmation.Visible = false;
                divPaymentIncomplete.Visible = true;
            }

            litGiftTotal.Text = _transaction.Amount.ToString();

            if ( _detailList.Count == 1 )
            {
                litMultiGift.Visible = false;
                litGiftTotal.Visible = false;
            }
            
            rptGiftConfirmation.DataSource = _detailList.ToDictionary(f => (string)f.Account.PublicName, f => (decimal)f.Amount);
            rptGiftConfirmation.DataBind();

            pnlDetails.Visible = false;
            pnlConfirm.Visible = true;
        }

        protected void btnGive_Click( object sender, EventArgs e )
        {
            // TODO give through payment gateway

            _transaction = (FinancialTransaction)ViewState["CachedTransaction"];
            _transactionService = new FinancialTransactionService();
            _transactionService.Save( _transaction, CurrentPersonId );

            litDateGift.Text = DateTime.Now.ToString( "f" );
            litGiftTotal2.Text = litGiftTotal.Text;
            litPaymentType2.Text = litPaymentType.Text;


            pnlConfirm.Visible = false;
            pnlComplete.Visible = true;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the campuses.
        /// </summary>
        protected void BindCampuses()
        {
            btnCampusList.Items.Clear();
            CampusService campusService = new CampusService();
            var items = campusService.Queryable().OrderBy( a => a.Name ).Select( a => a.Name ).Distinct().ToList();

            foreach ( string item in items )
            {
                btnCampusList.Items.Add( item );
            }

            btnCampusList.Title = "Select Campus";
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        protected void BindAccounts()
        {
            FinancialAccountService accountService = new FinancialAccountService();
            _transaction = new FinancialTransaction();            
            _transactionService.Save( _transaction, CurrentPersonId );

            var queryable = accountService.Queryable().Where( f => f.IsActive )
                .Distinct().OrderBy( f => f.Order );

            List<int> defaultAccounts = GetAttributeValue( "DefaultAccountstodisplay" ).Any()
                ? GetAttributeValue( "DefaultAccountstodisplay" ).Split( ',' ).ToList().Select( s => int.Parse( s ) ).ToList()
                : new List<int>( ( queryable.Select( f => f.Id ).ToList().FirstOrDefault() ) );

            if ( ( queryable.Count() - defaultAccounts.Count ) > 0 )
            {
                btnAddAccount.DataSource = queryable.Where( f => !defaultAccounts.Contains( f.Id ) )
                   .Select( f => f.PublicName ).ToList();
                btnAddAccount.DataBind();
                btnAddAccount.Title = "Add Another Gift";
                divAddAccount.Visible = true;
            }
            else
            {
                divAddAccount.Visible = false;
            }

            ViewState["CachedTransaction"] = _transaction;

            rptAccountList.DataSource = queryable.Where( f => defaultAccounts.Contains( f.Id ) )
                .ToDictionary( f => f.PublicName, f => Convert.ToDecimal( !f.IsActive ) );
            rptAccountList.DataBind();
        }

        /// <summary>
        /// Binds the person details if they're logged in.
        /// </summary>
        protected void BindPersonDetails()
        {
            GroupMemberService groupService = new GroupMemberService();
            LocationService locationService = new LocationService();
            GroupLocationService groupLocationService = new GroupLocationService();

            List<int> personGroups = groupService.Queryable()
                .Where( g => g.PersonId == CurrentPersonId )
                .Select( g => g.GroupId ).ToList();

            Location personLocation = groupLocationService.Queryable()
                .Where( g => personGroups.Contains( g.GroupId ) )
                .Select( g => g.Location )
                .ToList().FirstOrDefault();
            
            if ( personLocation != null )
            {
                txtFirstName.Value = CurrentPerson.FirstName.ToString();
                txtLastName.Value = CurrentPerson.LastName.ToString();
                txtAddress.Value = personLocation.Street1.ToString();
                txtCity.Value = personLocation.City.ToString();
                ddlState.Value = personLocation.State.ToString();
                txtZipcode.Value = personLocation.Zip.ToString();
                txtEmail.Value = CurrentPerson.Email.ToString();
            }
        }

        /// <summary>
        /// Binds the page control options.
        /// </summary>
        protected void BindOptions()
        {
            // bind frequency options
            var frequencyTypeGuid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_FREQUENCY );
            btnRecurrence.BindToDefinedType( DefinedTypeCache.Read( frequencyTypeGuid ) );
            
            // bind credit card options
            btnMonthExpiration.Items.Clear();
            btnYearExpiration.Items.Clear();

            btnMonthExpiration.DataSource = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.ToList().GetRange(0, 12);
            btnYearExpiration.DataSource = Enumerable.Range( (DateTime.Now.Year), 10).ToList();
            
            btnMonthExpiration.DataBind();
            btnYearExpiration.DataBind();
        }

        #endregion
    }
}