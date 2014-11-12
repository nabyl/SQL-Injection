using System;
using System.Collections.Generic;
using System.Text;

using gudusoft.gsqlparser;
using gudusoft.gsqlparser.Units;


namespace antiSQLInjection
{
	public enum ESQLInjectionType
	{
		syntax_error, always_true_condition, always_false_condition, comment_at_the_end_of_statement,
		stacking_queries, not_in_allowed_statement, union_set
	}

	public class TSQLInjection
	{
		private ESQLInjectionType type = ESQLInjectionType.syntax_error;

		public TSQLInjection(ESQLInjectionType pType)
		{
			this.type = pType;
			this.description = pType.ToString();
		}


		public ESQLInjectionType getType()
		{
			return type;
		}

		private String description = null;

		public String getDescription()
		{
			return description;
		}

		public void setDescription(String description)
		{
			this.description = description;
		}

	}

	public class TAntiSQLInjection
	{
		private TGSqlParser sqlParser = null;
		private String sqlText = null;
		private List<TSQLInjection> sqlInjections = null;
		private List<TSqlStatementType> enabledStatements = null;

		private Boolean  e_syntax_error = true;
		private Boolean e_always_true_condition = true;
		private Boolean e_always_false_condition = true;
		private Boolean e_comment_at_the_end_of_statement = true;
		private Boolean e_stacking_queries = true;
		private Boolean e_not_in_allowed_statement = true;
		private Boolean e_union_set = true;

		/**
		 * turn on/off the check of ESQLInjectionType.union_set
		 * default is on
		 * @param on
		 */
		public void check_union_set(Boolean on)
		{
			this.e_union_set = on;
		}

		/**
		 * turn on/off the check of ESQLInjectionType.not_in_allowed_statement
		 * default is on
		 * @param on
		 */
		public void check_not_in_allowed_statement(Boolean on)
		{
			this.e_not_in_allowed_statement = on;
		}

		/**
		 * turn on/off the check of ESQLInjectionType.stacking_queries
		 * default is on
		 * @param on
		 */
		public void check_stacking_queries(Boolean on)
		{
			this.e_stacking_queries = on;
		}

		/**
		 * turn on/off the check of ESQLInjectionType.comment_at_the_end_of_statement
		 * default is on
		 * @param on
		 */
		public void check_comment_at_the_end_of_statement(Boolean on)
		{
			this.e_comment_at_the_end_of_statement = on;
		}

		/**
		 * turn on/off the check of ESQLInjectionType.always_false_condition
		 * default is on
		 * @param on
		 */
		public void check_always_false_condition(Boolean on)
		{
			this.e_always_false_condition = on;
		}

		/**
		 * turn on/off the check of ESQLInjectionType.always_true_condition
		 * default is on
		 * @param on
		 */
		public void check_always_true_condition(Boolean on)
		{
			this.e_always_true_condition = on;
		}

		public List<TSQLInjection> getSqlInjections()
		{
			if (this.sqlInjections == null)
			{
				this.sqlInjections = new List<TSQLInjection>();
			}
			return sqlInjections;
		}

		public TAntiSQLInjection(TDbVendor dbVendor)
		{
			this.sqlParser = new TGSqlParser(dbVendor);
			this.enabledStatements = new List<TSqlStatementType>();
			this.enabledStatements.Add(TSqlStatementType.sstSelect);
		}

		/**
		 * add a type of sql statement that allowed to be executed in database.
		 * @param sqltype
		 */
		public void enableStatement(TSqlStatementType sqltype)
		{
			this.enabledStatements.Add(sqltype);
		}

		/**
		 * get a list of sql statement type that allowed to be executed in database.
		 * @return
		 */
		public List<TSqlStatementType> getEnabledStatements()
		{
			return enabledStatements;
		}

		/**
		 * disable a type of sql statement that allowed to be executed in database.
		 * @param sqltype
		 */
		public void disableStatement(TSqlStatementType sqltype)
		{
			for (int i = this.enabledStatements.Count - 1; i >= 0; i--)
			{
				if (this.enabledStatements[i] == sqltype)
				{
					this.enabledStatements.RemoveAt(i);
				}
			}
		}

		/**
		 * Check is sql was injected or not.
		 * @param sql
		 * @return if return true, use this.getSqlInjections() to get detailed information about sql injection.
		 */
		public Boolean isInjected(String sql)
		{
			Boolean ret = false;
			this.sqlText = sql;
			this.sqlParser.SqlText.Text = this.sqlText;
			this.getSqlInjections().Clear();
			int i = this.sqlParser.Parse();
			if (i == 0)
			{
				ret = ret | isInjected_always_false_condition();
				ret = ret | isInjected_always_true_condition();
				ret = ret | isInjected_comment_at_the_end_statement();
				ret = ret | isInjected_stacking_queries();
				ret = ret | isInjected_allowed_statement();
				ret = ret | isInjected_union_set();
			}
			else
			{
				TSQLInjection s = new TSQLInjection(ESQLInjectionType.syntax_error);
				s.setDescription(this.sqlParser.ErrorMessages);
				this.getSqlInjections().Add(s);
				ret = true;
			}

			return ret;
		}

		/// <summary>
		/// This function is not implemented yet
		/// </summary>
		/// <returns></returns>
		private Boolean isInjected_always_true_condition(){
			Boolean ret = false;
			if (!this.e_always_true_condition) {return false;}
			if (this.sqlParser.SqlStatements.Count() == 0) {return ret;}
			if (this.sqlParser.SqlStatements[0].WhereClause != null){
				GEval e = new GEval(null);
				this.sqlParser.SqlStatements[0].WhereClause.PostOrderTraverse(e.eavl);
				Object t = e.getValue();
				if (t is Boolean){
					if (((Boolean) t) == true){
						this.getSqlInjections().Add(new TSQLInjection(ESQLInjectionType.always_true_condition));
						ret = true;
					}
				}
			}
			return ret;
		}

		/// <summary>
		/// This function is not implemented yet
		/// </summary>
		/// <returns></returns>
		private Boolean isInjected_always_false_condition(){
            Boolean ret = false;
            if (!this.e_always_true_condition) { return false; }
            if (this.sqlParser.SqlStatements.Count() == 0) { return ret; }
            if (this.sqlParser.SqlStatements[0].WhereClause != null)
            {
                GEval e = new GEval(null);
                this.sqlParser.SqlStatements[0].WhereClause.PostOrderTraverse(e.eavl);
                Object t = e.getValue();
                if (t is Boolean)
                {
                    if (((Boolean)t) == false)
                    {
                        this.getSqlInjections().Add(new TSQLInjection(ESQLInjectionType.always_false_condition));
                        ret = true;
                    }
                }
            }
			return ret;
		}

		private Boolean isInjected_comment_at_the_end_statement()
		{
			Boolean ret = false;
			if (!this.e_comment_at_the_end_of_statement) { return false; }
			TSourceToken st = null;
			for (int j = this.sqlParser.SourceTokenList.Count() - 1; j >= 0; j--)
			{
				st = this.sqlParser.SourceTokenList[j];
				if ((st.TokenType == TTokenType.ttWhiteSpace)
				    || (st.TokenType == TTokenType.ttReturn)
				   )
				{ continue; }
				else
				{
					break;
				}
			}
			if ((st.TokenType == TTokenType.ttDoublehyphenComment) || (st.TokenType == TTokenType.ttSlashStarComment))
			{
				this.getSqlInjections().Add(new TSQLInjection(ESQLInjectionType.comment_at_the_end_of_statement));
				ret = true;
			}
			return ret;
		}

		private Boolean isInjected_stacking_queries()
		{
			Boolean ret = false;
			if (!this.e_stacking_queries) { return false; }
			if (this.sqlParser.SqlStatements.Count() > 1)
			{
				this.getSqlInjections().Add(new TSQLInjection(ESQLInjectionType.stacking_queries));
				ret = true;
			}
			return ret;
		}


		private Boolean isInjected_allowed_statement()
		{
			Boolean ret = false;
			if (!this.e_not_in_allowed_statement) { return false; }
			for (int j = 0; j < this.sqlParser.SqlStatements.Count(); j++)
			{
				if (!this.isAllowedStatement(this.sqlParser.SqlStatements[j].SqlStatementType))
				{

					TSQLInjection s = new TSQLInjection(ESQLInjectionType.not_in_allowed_statement);
					s.setDescription(this.sqlParser.SqlStatements[j].SqlStatementType.ToString());
					this.getSqlInjections().Add(s);

					ret = ret | true;
				};

			}
			return ret;
		}


		private Boolean isInjected_union_set()
		{
			Boolean ret = false;
			if (!this.e_union_set) { return false; }
			if (this.sqlParser.SqlStatements.Count() == 0) { return ret; }
			TCustomSqlStatement stmt = this.sqlParser.SqlStatements[0];
			if (stmt.SqlStatementType != TSqlStatementType.sstSelect) { return ret; }
			TSelectSqlStatement select = (TSelectSqlStatement)stmt;
			if (select.SelectSetType != TSelectSetType.sltNone)
			{
				this.getSqlInjections().Add(new TSQLInjection(ESQLInjectionType.union_set));
				ret = true;
			}
			return ret;
		}



		private Boolean isAllowedStatement(TSqlStatementType pType)
		{
			Boolean ret = false;
			for (int i = 0; i < this.enabledStatements.Count; i++)
			{
				if (this.enabledStatements[i] == pType)
				{
					ret = true;
					break;
				}
			}
			return ret;
		}

	}
}
