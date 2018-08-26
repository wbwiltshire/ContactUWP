using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeagullConsulting.ContactUWP.Data;
using SeagullConsulting.ContactUWP.Data.Interfaces;
using SeagullConsulting.ContactUWP.Data.POCO;

namespace SeagullConsulting.ContactUWP.Data.Repository
{

    public class ContactRepository : RepositoryBase<Contact>, IRepository<Contact>
    {

        private const string FINDALLCOUNT_STMT = "SELECT COUNT(Id) FROM Contact WHERE Active=1";
        private const string FINDALL_STMT = "SELECT Id,FirstName,LastName,Address1,Address2,Notes,ZipCode,HomePhone,WorkPhone,CellPhone,EMail,CityId,Active,ModifiedDt,CreateDt FROM Contact WHERE Active=1";
        private const string FINDALLVIEW_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, CityName, StateId, StateName, Active, ModifiedDt, CreateDt FROM vwFindAllContactView";
        private const string FINDALLPAGER_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, Active, ModifiedDt, CreateDt FROM Contact WHERE Active=1 ORDER BY Id OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;";
        private const string FINDALLVIEWPAGER_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId,  CityName, StateId, StateName, Active, ModifiedDt, CreateDt FROM vwFindAllContactView ORDER BY Id OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;";
        private const string FINDBYPK_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, Active, ModifiedDt, CreateDt FROM Contact WHERE Id =@pk AND Active=1";
        private const string FINDBYPKVIEW_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, CityName, StateId, StateName, Active, ModifiedDt, CreateDt FROM vwFindAllContactView WHERE Id =@pk AND Active = 1 ORDER BY Id ";
        private const string ADD_STMT = "INSERT INTO Contact (FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, Active, ModifiedDt, CreateDt) VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, 1, GETDATE(), GETDATE()); SELECT CAST(SCOPE_IDENTITY() AS INT);";
        private const string UPDATE_STMT = "UPDATE Contact SET FirstName=@p1, LastName=@p2, Address1=@p3, Address2=@p4, Notes=@p5, ZipCode=@p6, HomePhone=@p7, WorkPhone=@p8, CellPhone=@p9, EMail=@p10, CityId=@p11, Active=1, ModifiedDt=GETDATE() WHERE Id =@pk AND Active=1";
        private const string DELETE_STMT = "UPDATE Contact SET Active=0, ModifiedDt=GETDATE() WHERE Id =@pk";
        private const string ORDERBY_STMT = " ORDER BY ";
        private const string FINDALL_PAGEDPROC = "uspFindAllContactPaged";
        private const string FINDALL_PAGEDVIEWPROC = "uspFindAllContactViewPaged";
        private const string ADD_PROC = "uspAddContact";
        private const string UPDATE_PROC = "uspUpdateContact";
        private const string NONQUERY_PROC = "uspNonQuery";
        private const string STORED_PROC = "uspStoredProc";

        private ILogger logger;

        #region ctor
        //Default constructor calls the base ctor
        public ContactRepository(AppSettingsConfiguration s, ILogger l, DBConnection d) :
            base(s, l, d)
        { Init(l); }
        public ContactRepository(AppSettingsConfiguration s, ILogger l, UnitOfWork uow, DBConnection d) :
            base(s, l, uow, d)
        { Init(l); }


        private void Init(ILogger l)
        {
            logger = l;
            //Mapper = new ContactMapper();
            OrderBy = "Id";
        }
        #endregion

        #region FindAll
        public override async Task<ICollection<Contact>> FindAll()
        {
            CMDText = FINDALL_STMT;
            CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new ContactMapToObject(logger);
            return await base.FindAll();
        }
        #endregion

        #region FindAll(IPager)
        public async Task<IPager<Contact>> FindAll(IPager<Contact> pager)
        {
            string storedProcedure = String.Empty;

            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new ContactMapToObject(logger);

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == FINDALL_PAGEDPROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = String.Format(FINDALLPAGER_STMT, pager.PageSize * pager.PageNbr, pager.PageSize);
                pager.Entities = await base.FindAll();
            }
            else
            {
                SqlCommandType = Constants.DBCommandType.SPROC;
                CMDText = storedProcedure;
                pager.Entities = await base.FindAllPaged(pager.PageSize * pager.PageNbr, pager.PageSize);
            }

            CMDText = FINDALLCOUNT_STMT;
            pager.RowCount = await base.FindAllCount();
            return pager;
        }
        #endregion

        #region FindAllView
        public async Task<ICollection<Contact>> FindAllView()
        {
            CMDText = FINDALLVIEW_STMT;
            CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new ContactMapToObjectView(logger);
            return await base.FindAll();
        }
        #endregion

        #region FindAllView(IPager)
        public async Task<IPager<Contact>> FindAllView(IPager<Contact> pager)
        {
            string storedProcedure = String.Empty;

            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new ContactMapToObjectView(logger);

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == FINDALL_PAGEDVIEWPROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = String.Format(FINDALLVIEWPAGER_STMT, pager.PageSize * pager.PageNbr, pager.PageSize);
                pager.Entities = await base.FindAll();
            }
            else
            {
                SqlCommandType = Constants.DBCommandType.SPROC;
                CMDText = storedProcedure;
                pager.Entities = await base.FindAllPaged(pager.PageSize * pager.PageNbr, pager.PageSize);
            }

            CMDText = FINDALLCOUNT_STMT;
            pager.RowCount = await base.FindAllCount();
            return pager;
        }
        #endregion

        #region FindByPK
        public override async Task<Contact> FindByPK(IPrimaryKey pk)
        {
            CMDText = FINDBYPK_STMT;
            MapToObject = new ContactMapToObject(logger);
            return await base.FindByPK(pk);
        }
        #endregion

        #region FindViewByPK
        public async Task<Contact> FindViewByPK(IPrimaryKey pk)
        {
            CMDText = FINDBYPKVIEW_STMT;
            MapToObject = new ContactMapToObjectView(logger);
            return await base.FindByPK(pk);
        }
        #endregion

        #region Add
        public async Task<object> Add(Contact entity)
        {
            string storedProcedure = String.Empty;
            object result;
            int key = 0;

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == ADD_PROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = ADD_STMT;
            }
            else
            {
                SqlCommandType = Constants.DBCommandType.SPROC;
                CMDText = storedProcedure;
            }
            MapFromObject = new ContactMapFromObject(logger);
            result = await base.Add(entity, entity.PK);
            if (result != null)
                key = Convert.ToInt32(result);

            return key;
        }
        #endregion

        #region Update
        public async Task<int> Update(Contact entity)
        {
            string storedProcedure = String.Empty;

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == UPDATE_PROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = UPDATE_STMT;
            }
            else
            {
                SqlCommandType = Constants.DBCommandType.SPROC;
                CMDText = storedProcedure;
            }
            MapFromObject = new ContactMapFromObject(logger);
            return await base.Update(entity, entity.PK);
        }
        #endregion

        #region Delete
        public async Task<int> Delete(PrimaryKey pk)
        {
            CMDText = DELETE_STMT;
            return await base.Delete(pk);
        }
        #endregion

        #region ExecNonQuery
        public async Task<int> NonQuery()
        {
            int rows = 0;
            string storedProcedure = String.Empty;
            IList<SqlParameter> parms = new List<SqlParameter>();

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == NONQUERY_PROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = FINDALLCOUNT_STMT;
                rows = await base.ExecNonQuery(parms);
            }
            else
            {
                CMDText = storedProcedure;
                SqlCommandType = Constants.DBCommandType.SPROC;
                rows = await base.ExecNonQuery(parms);
            }
            return rows;
        }
        #endregion

        #region ExecStoredProc
        public async Task<int> StoredProc(int id)
        {
            int rows = 0;
            string storedProcedure = String.Empty;
            IList<SqlParameter> parms = new List<SqlParameter>();

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == STORED_PROC);
            if (storedProcedure == null)
            {
                CMDText = STORED_PROC;
                SqlCommandType = Constants.DBCommandType.SPROC;
                parms.Add(new SqlParameter("@pk", id));
                rows = await base.ExecStoredProc(parms);
            }
            else
            {
                CMDText = storedProcedure;
                SqlCommandType = Constants.DBCommandType.SPROC;
                parms.Add(new SqlParameter("@pk", id));
                rows = await base.ExecStoredProc(parms);
            }
            return rows;
        }
        #endregion
    }
    }
