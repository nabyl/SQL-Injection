using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLIA.Model;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.Units;

namespace SQLIA.Scanner
{
    //yoosuf nabeel solih 
    //11-Nov-2014
    public class SQLValidator
    {

        #region properties
        public bool InjectionsFound { get; set; }

        public string statement { get; set; }
        private string[] tokens;

        public List<String> Messages { get; set; } //stores messages regarding the sql statement under test 
        public List<int> AttackTypes { get; set; }

        public SQLValidator(string statement)
        {
            this.InjectionsFound = false;
            this.statement = statement;
            this.AttackTypes = new List<int>();
            this.Messages = new List<string>();

        }

        #endregion


        /// <summary>
        /// Assumes SQLValidator is instantiated with sql statement.
        /// </summary>
        /// <returns></returns>
        public void CheckInjections()
        {
            //TAntiSQLInjection anti = new TAntiSQLInjection(TDbVendor.DbVOracle);

            //String msg = "";
            //if (anti.isInjected(this.statement))
            //{
            //    this.InjectionsFound = true;
            //    msg = "SQL injected found:";
            //    for (int i = 0; i < anti.getSqlInjections().Count; i++)
            //    {
            //        Messages.Add(anti.getSqlInjections()[i].getDescription());

            //        if (anti.getSqlInjections()[i].getType() == ESQLInjectionType.always_true_condition || anti.getSqlInjections()[i].getType() == ESQLInjectionType.always_false_condition)
            //        {
            //            AttackTypes.Add(ModelConstants.AttackVectorTypes.BOOLEAN);
            //        }
            //        else if (anti.getSqlInjections()[i].getType() == ESQLInjectionType.syntax_error)
            //        {
            //            AttackTypes.Add(ModelConstants.AttackVectorTypes.ERROR_BASED);
            //        }
            //        else if (anti.getSqlInjections()[i].getType() == ESQLInjectionType.stacking_queries)
            //        {
            //            AttackTypes.Add(ModelConstants.AttackVectorTypes.PIGGY_BACK_QUERIES);
            //        }
            //        else if (anti.getSqlInjections()[i].getType() == ESQLInjectionType.union_set)
            //        {
            //            AttackTypes.Add(ModelConstants.AttackVectorTypes.UNION);
            //        }
            //        else if (anti.getSqlInjections()[i].getType() == ESQLInjectionType.comment_at_the_end_of_statement)
            //        {
            //            AttackTypes.Add(ModelConstants.AttackVectorTypes.COMMENTS);
            //        }


            //        //msg = msg + Environment.NewLine + ("type: " + anti.getSqlInjections()[i].getType() + ", description: " + anti.getSqlInjections()[i].getDescription());
            //    }
            //}
            //else
            //{
            //    this.InjectionsFound = false;
            //}
            HasCommentTokens();

            //remove comment tokens and test all further injections
            this.statement = this.statement.Replace("--", "");
            this.statement = this.statement.Replace("/*", "");
            this.statement = this.statement.Replace("*/", "");


            HasPiggyBackQueries();
            HasATimeDelay();
            HasABooleanExploitation();
            HasAUnionExploitation();
            HasOutOfBandExploitation();
            HasNotAllowedTokens();

            IsValid();

        }

        #region Methods


        /// <summary>
        /// Checks the sql string under test is syntactically correct or not. 
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            bool valid = true;

            // var parseResult = Microsoft.SqlServer.Management.SqlParser.Parser.Parser.Parse(statement);


            TDbVendor dbv = TDbVendor.DbVMysql;
            TGSqlParser sqlparser = new TGSqlParser(dbv);

            sqlparser.SqlText.Text = this.statement;

            int iRet = sqlparser.Parse();

            if (iRet != 0)
            {
                this.InjectionsFound = false;
                valid = false;
                AttackTypes.Add(ModelConstants.AttackVectorTypes.ERROR_BASED);

                for (int i = 0; i < sqlparser.ErrorCount; i++)
                {
                    Messages.Add(sqlparser.SyntaxErrors[i].Hint);
                }

            }

            return valid;
        }

        /// <summary>
        /// Checks if the sql string under test has more than one separate sql statements
        /// </summary>
        /// <returns></returns>
        public bool HasPiggyBackQueries()
        {
            bool has = false;
            TDbVendor dbv = TDbVendor.DbVMysql;
            TGSqlParser sqlparser = new TGSqlParser(dbv);
            sqlparser.SqlText.Text = this.statement;
            int iRet = sqlparser.Parse();


            if (sqlparser.SqlStatements.Count() > 1)
            {
                has = true;
                this.InjectionsFound = false;
                AttackTypes.Add(ModelConstants.AttackVectorTypes.PIGGY_BACK_QUERIES);
                Messages.Add("Stacking queries found. ");
            }

            return has;
        }
        /// <summary>
        /// This function checks for any comment tokens in the sql statement
        /// </summary>
        /// <returns></returns>
        public bool HasCommentTokens()
        {
            bool has = false;

            if (statement.Contains("--") || statement.Contains("/*") || statement.Contains("*/"))
            {
                this.InjectionsFound = false;
                has = true;
                AttackTypes.Add(ModelConstants.AttackVectorTypes.COMMENTS);

                if (statement.Contains("/*"))
                    Messages.Add("/* token found. ");
                else if (statement.Contains("--"))
                    Messages.Add("-- token found. ");
            }

            return has;
        }


        /// <summary>
        /// This function checks for any time delay tokens, here it is testing for 'sleep' and 'waitfor'
        /// </summary>
        /// <returns></returns>
        public bool HasATimeDelay()
        {
            bool has = false;

            if (statement.ToLower().Contains("sleep") || statement.ToLower().Contains("waitfor"))
            {
                this.InjectionsFound = false;
                has = true;
                AttackTypes.Add(ModelConstants.AttackVectorTypes.TIME_DELAY);
                Messages.Add("sleep/waitfor token found. ");
            }

            return has;
        }

        /// <summary>
        /// This functions checks for always true condition and always false conditions. 
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public bool HasABooleanExploitation()
        {
            bool has = false;

            //extract 1=1 like part from the string for examination 
            //find all the boolean expressions 

            this.tokenizeStatement();
            var tokenBoolean = new List<string> { ">", "<", ">=", "<=", "<>", "!=", "!>", "<!", "=", "like", "not like", " in " };
            this.tokenizeStatement();

            if (this.statement.ToLower().Contains(" in "))
            {
                int x = 0;
            }


            for (int i = 0; i < this.tokens.Length; i++)
            {
                foreach (var item in tokenBoolean)
                {

                    if (tokens[i].ToLower().Contains(item) || tokens[i].ToLower().Equals(item.Trim()))
                    {
                        string tokenUnderTest = this.tokens[i];
                        NCalc.Expression evaluationString = new NCalc.Expression(tokenUnderTest);


                        switch (item)
                        {
                            case "like":
                                //SELECT * FROM members WHERE username = 'admin' or "hacker" like "hack%"--
                                string firstParameter = this.tokens[i - 1].Replace("\"", "").Replace("%", "");
                                string secondParameter = this.tokens[i + 1].Replace("\"", "").Replace("%", ""); ;

                                if (firstParameter.Contains(secondParameter))
                                {
                                    has = true;
                                    this.InjectionsFound = true;
                                    Messages.Add("like evaluates to true.");
                                    AttackTypes.Add(ModelConstants.AttackVectorTypes.BOOLEAN);
                                }

                                break;


                            case " in ":

                                firstParameter = this.tokens[i - 1].Replace("\"", "").Replace("%", "").Replace("#", "").Replace("#", "").Replace("(", "").Replace(")", "").Replace("x", "*");
                                secondParameter = this.tokens[i + 1].Replace("\"", "").Replace("%", "").Replace("#", "").Replace("(", "").Replace(")", "").Replace("x", "*");

                                string[] splitCharacters = { " ", "," };
                                string[] first = firstParameter.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);
                                string[] second = secondParameter.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);


                                for (int j = 0; j < first.Length; j++)
                                {
                                    evaluationString = new NCalc.Expression(first[j]);

                                    try
                                    {
                                        var obj = evaluationString.Evaluate();
                                        int intResult = 0;

                                        if (int.TryParse(obj.ToString(), out intResult)) 
                                        {
                                            first[j] = intResult.ToString();
                                        }
                                    }
                                    catch (Exception ex) {}
                                }

                                for (int k = 0; k < first.Length; k++)
                                {
                                    evaluationString = new NCalc.Expression(second[k]);

                                    try
                                    {
                                        var obj = evaluationString.Evaluate();
                                        int intResult = 0;

                                        if (int.TryParse(obj.ToString(), out intResult))
                                        {
                                            second[k] = intResult.ToString();
                                        }
                                    }
                                    catch (Exception ex) { }
                                }

                                bool foundInList = false;
                                foreach(var value in first)
                                {
                                    // check value is in second list 
                                    if (second.Contains(value))
                                    {
                                        foundInList = true;
                                        has = true;
                                        this.InjectionsFound = true;
                                        Messages.Add("always true condition found");
                                        AttackTypes.Add(ModelConstants.AttackVectorTypes.BOOLEAN);
                                    }
                                }

                                


                                break;
                            default:

                                if (tokenUnderTest.Length == 1)
                                {
                                    tokenUnderTest = this.tokens[i - 1] + tokenUnderTest + this.tokens[i + 1];
                                }

                                try
                                {
                                    tokenUnderTest = tokenUnderTest.Replace("x", "*");


                                    evaluationString = new NCalc.Expression(tokenUnderTest);
                                    if (true == (bool)evaluationString.Evaluate())
                                    {
                                        has = true;
                                        this.InjectionsFound = true;
                                        Messages.Add("always true condition found");
                                        AttackTypes.Add(ModelConstants.AttackVectorTypes.BOOLEAN);
                                    }
                                    else if (false == (bool)evaluationString.Evaluate())
                                    {
                                        has = true;
                                        this.InjectionsFound = true;
                                        Messages.Add("always false condition found");
                                        AttackTypes.Add(ModelConstants.AttackVectorTypes.BOOLEAN);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                break;
                        }

                    }


                }
            }


            //check 

            return has;
        }

        private void tokenizeStatement()
        {
            string[] seps = { " " };//, "select", "from", "where", "*", ",", "insert", "update","=", ">", "<", ">=", "<=", "<>", "!=", "!>", "<!", "and", "any", "between", "exists", "like", "in", "not", "or", "some" };
            tokens = this.statement.Split(seps, StringSplitOptions.RemoveEmptyEntries);
        }


        private bool HasBooleanOperator(out int index)
        {
            index = 0;
            var tokenBoolean = new List<string> { ">", "<", ">=", "<=", "<>", "!=", "!>", "<!", "=", "and", "any", "between", "exists", "like", "in", "not", "some" };
            this.tokenizeStatement();

            for (int i = 0; i < this.tokens.Length; i++)
            {
                if (tokenBoolean.Contains(tokens[i]))
                {
                    index = i;
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        ///  Checks for the following tokens Union = 1, Union All, Minus, Intersect, Intersect All, Except, Except All,
        /// </summary>
        /// <returns></returns>
        public bool HasAUnionExploitation()
        {
            bool has = false;

            if (statement.ToLower().Contains("select") &&
                ((statement.ToLower().Contains("union") ||
                statement.ToLower().Contains("union all") ||
                statement.ToLower().Contains("minus") ||
                statement.ToLower().Contains("intersect") ||
                statement.ToLower().Contains("intersect all") ||
                statement.ToLower().Contains("except") ||
                 statement.ToLower().Contains("except all"))
                ))
            {
                // set command found, 
                // but need to ensure if the set command is selecting from another table or view compared to the first select. 
                has = true;
                this.InjectionsFound = false;
                Messages.Add("union set token found. ");
                AttackTypes.Add(ModelConstants.AttackVectorTypes.UNION);
            }



            return has;
        }


        /// <summary>
        /// This function checks for any out of band code
        /// --- mysql - into outfile , BENCHMARK
        /// --- mssql - xp_ 
        /// --- oracle
        /// </summary>
        /// <returns></returns>
        public bool HasOutOfBandExploitation()
        {
            bool has = false;
            var sql_functions = new List<string> { "into outfile", "load_file", "benchmark", "xp_" };

            foreach (var function in sql_functions)
            {
                if (this.statement.ToLower().Contains(function))
                {
                    Messages.Add(function + " token found. ");
                    has = true;
                    this.InjectionsFound = false;
                    AttackTypes.Add(ModelConstants.AttackVectorTypes.OUT_OF_BAND);
                    break;
                }
            }

            return has;
        }
        public bool HasNotAllowedTokens()
        {
            bool has = false;
            var sql_functions = new List<string> { "@@version", "drop", "alter" };

            foreach (var function in sql_functions)
            {
                if (this.statement.ToLower().Contains(function))
                {
                    Messages.Add(function + " token found. ");
                    has = true;
                    this.InjectionsFound = false;
                    AttackTypes.Add(ModelConstants.AttackVectorTypes.NOT_ALLOWED_QUERY);
                    break;
                }
            }

            return has;
        }

        #endregion
    }
}
