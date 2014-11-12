/*
 * Created by SharpDevelop.
 * User: Feeling
 * Date: 2012/1/3
 * Time: 0:58
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;

namespace antiSQLInjection
{
	/// <summary>
	/// Description of GContext.
	/// </summary>
	public interface GContext
	{
		void setVars(Hashtable vars);
		Hashtable getVars();
	}
}
