/*
 * Created by SharpDevelop.
 * User: ekr
 * Date: 29/08/2013
 * Time: 13:04
 * 
 */
using System;

/**
 * 
 * <see cref="http://stackoverflow.com/questions/1522605/using-extension-methods-in-net-2-0"></see>
 **/
 
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
         | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute {}
}
