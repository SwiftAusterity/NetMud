﻿using NetMud.Data.Reference;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Data;
using System.Text;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for NPCs
    /// </summary>
    public class NonPlayerCharacter : EntityBackingDataPartial, INonPlayerCharacter
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(Game.Intelligence); }
        }

        /// <summary>
        /// Gender data string for NPCs
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// "family name" for NPCs
        /// </summary>
        public string SurName { get; set; }

        public IRace RaceData { get; set; }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            var height = RaceData.Head.Model.Height + RaceData.Torso.Model.Height + RaceData.Legs.Item1.Model.Height;
            var length = RaceData.Torso.Model.Length;
            var width = RaceData.Torso.Model.Width;

            return new Tuple<int, int, int>(height, length, width);
        }

        /// <summary>
        /// Full name to refer to this NPC with
        /// </summary>
        /// <returns>the full name string</returns>
        public string FullName()
        {
            return string.Format("{0} {1}", Name, SurName);
        }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public override void Fill(DataRow dr)
        {
            ID = DataUtility.GetFromDataRow<long>(dr, "ID");
            Created = DataUtility.GetFromDataRow<DateTime>(dr, "Created");
            LastRevised = DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised");
            Name = DataUtility.GetFromDataRow<string>(dr, "Name");

            SurName = DataUtility.GetFromDataRow<string>(dr, "SurName"); ;
            Gender = DataUtility.GetFromDataRow<string>(dr, "Gender"); ;

            var raceId = DataUtility.GetFromDataRow<long>(dr, "Race"); ;
            RaceData = ReferenceWrapper.GetOne<Race>(raceId);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            INonPlayerCharacter returnValue = default(INonPlayerCharacter);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[NonPlayerCharacter]([SurName], [Name], [Gender], [Race])");
            sql.AppendFormat(" values('{0}','{1}','{2}', {3}, {4}, {5}, {6}, '{7}', {8})"
                , SurName, Name, Gender, RaceData.ID);
            sql.Append(" select * from [dbo].[NonPlayerCharacter] where ID = Scope_Identity()");

            try
            {
                var ds = SqlWrapper.RunDataset(sql.ToString(), CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        Fill(dr);
                        returnValue = this;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return returnValue;
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public override bool Remove()
        {
            var sql = new StringBuilder();
            sql.AppendFormat("delete from [dbo].[NonPlayerCharacter] where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save()
        {
            var sql = new StringBuilder();
            sql.Append("update [dbo].[NonPlayerCharacter] set ");
            sql.AppendFormat(" [SurName] = '{0}' ", SurName);
            sql.AppendFormat(" , [Name] = '{0}' ", Name);
            sql.AppendFormat(" , [Gender] = '{0}' ", Gender);
            sql.AppendFormat(" , [Race] = {0} ", RaceData.ID);
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }
    }
}
