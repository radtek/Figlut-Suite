﻿namespace Figlut.Server.Toolkit.Data.QueryRunners
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public interface ISqlQueryRunner
    {
        #region Methods

        SqlQueryRunnerOutput ExecuteQuery(SqlQueryRunnerInput input);

        #endregion //Methods
    }
}
