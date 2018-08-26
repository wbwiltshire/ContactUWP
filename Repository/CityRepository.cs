/******************************************************************************************************
 *This class was generated on 04/30/2014 09:06:10 using Repository Builder version 0.9. *
 *The class was generated from Database: Customer and Table: City.  *
******************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeagullConsulting.ContactUWP.Data;
using SeagullConsulting.ContactUWP.Data.Interfaces;
using SeagullConsulting.ContactUWP.Data.POCO;

namespace SeagullConsulting.ContactUWP.Data.Repository
{

    public class CityRepository : RepositoryBase<City>, IRepository<City>
    {
        private const string FINDALLCOUNT_STMT = "SELECT COUNT(Id) FROM City WHERE Active=1";
        private const string FINDALL_STMT = "SELECT Id,Name,StateId,Active,ModifiedDt,CreateDt FROM City WHERE Active=1";
        private const string FINDALLPAGER_STMT = "SELECT Id,Name,StateId,Active,ModifiedDt,CreateDt FROM City WHERE Active=1 ORDER BY Id OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";
        private const string FINDALLVIEW_STMT = "SELECT Id,Name,StateId,StateName,Active,ModifiedDt,CreateDt FROM vwFindAllCityView ORDER BY Id";
        private const string FINDALLVIEWPAGER_STMT = "SELECT Id,Name,StateId,StateName,Active,ModifiedDt,CreateDt FROM vwFindAllCityView ORDER BY Id OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";
        private const string FINDBYPK_STMT = "SELECT Id, Name, StateId, Active, ModifiedDt, CreateDt FROM City WHERE Id =@pk AND Active=1";
        private const string FINDBYPKVIEW_STMT = "SELECT Id,Name,StateId,StateName,Active,ModifiedDt,CreateDt FROM vwFindAllCityView WHERE Id =@pk AND Active=1";
        private const string ADD_STMT = "INSERT INTO City (Name, StateId, Active, ModifiedDt, CreateDt) VALUES (@p1, @p2, 1, GETDATE(), GETDATE()); SELECT CAST(SCOPE_IDENTITY() AS INT)";
        private const string UPDATE_STMT = "UPDATE City SET Name=@p1, StateId=@p2, Active=1, ModifiedDt=GETDATE() WHERE Id =@pk AND Active=1";
        private const string DELETE_STMT = "UPDATE City SET Active=0, ModifiedDt=GETDATE() WHERE Id =@pk";
        private const string ORDERBY_STMT = " ORDER BY ";
        private const string FINDALL_PAGEDPROC = "uspFindAllCityPaged";
        private const string FINDALL_PAGEDVIEWPROC = "uspFindAllCityViewPaged";
        private const string ADD_PROC = "uspAddCity";
        private const string UPDATE_PROC = "uspUpdateCity";

        private ILogger logger;

        #region ctor
        //Default constructor calls the base ctor
        public CityRepository(AppSettingsConfiguration s, ILogger l, DBConnection d) :
            base(s, l, d)
        { Init(l); }
        public CityRepository(AppSettingsConfiguration s, ILogger l, UnitOfWork uow, DBConnection d) :
            base(s, l, uow, d)
        { Init(l); }

        private void Init(ILogger l)
        {
            logger = l;
            //Mapper = new CityMapper();
            OrderBy = "Id";
        }
        #endregion

        #region FindAll
        public override async Task<ICollection<City>> FindAll()
        {
            CMDText = FINDALL_STMT;
            CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new CityMapToObject(logger);
            return await base.FindAll();
        }
        #endregion

        #region FindAll(Pager)
        public async Task<IPager<City>> FindAll(IPager<City> pager)
        {
            string storedProcedure = String.Empty;

            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new CityMapToObject(logger);

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
        public async Task<ICollection<City>> FindAllView()
        {
            CMDText = FINDALLVIEW_STMT;
            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new CityMapToObjectView(logger);
            return await base.FindAll();
        }
        #endregion

        #region FindAllView(Pager)
        public async Task<IPager<City>> FindAllView(IPager<City> pager)
        {
            string storedProcedure = String.Empty;

            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new CityMapToObjectView(logger);

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
        public override async Task<City> FindByPK(IPrimaryKey pk)
        {
            CMDText = FINDBYPK_STMT;
            MapToObject = new CityMapToObject(logger);
            return await base.FindByPK(pk);
        }
        #endregion

        #region Add
        public async Task<object> Add(City entity)
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
            MapFromObject = new CityMapFromObject(logger);
            result = await base.Add(entity, entity.PK);
            if (result != null)
                key = Convert.ToInt32(result);
            return key;
        }
        #endregion

        #region Update
        public async Task<int> Update(City entity)
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
            MapFromObject = new CityMapFromObject(logger);
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


    }
}


