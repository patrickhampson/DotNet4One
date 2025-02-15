﻿//
//  OneClient.cs
//
//  Author:
//       Enguerran Boissier <enguerran.boissier@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using CookComputing.XmlRpc;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Collections.Generic;

/*!

\defgroup OpenNebulaXMLRPC OpenNebula XML-RPC Interface
@{
    The XML-RPC interface is the primary interface for OpenNebula, and it exposes all the functionality to interface the OpenNebula daemon. 
    Through the XML-RPC interface, it is possible to control and manage any OpenNebula resource, including virtual machines, networks, images, users, hosts and clusters.

    \xrefitem cptype_int "Interfaces" "Interfaces"

    \xrefitem api API API [OpenNebula XML-RPC reference guide](http://docs.opennebula.org/4.12/integration/system_interfaces/api.html).

@}

*/

namespace Terradue.OpenNebula {
    /// <summary>
    /// DotNet4One Client calling XML-RPC requests exposed by an OpenNebula server.
    /// Constructor and common function between all XML-RPC commands.
    /// </summary>
    public partial class OneClient {

        /// <summary>
        /// Gets or sets the proxy URL.
        /// </summary>
        /// <value>The proxy URL.</value>
        private string ProxyUrl { get; set; }

        /// <summary>
        /// Gets or sets the admin username.
        /// </summary>
        /// <value>The admin username.</value>
        private string AdminUsername { get; set; }

        /// <summary>
        /// Gets or sets the admin password.
        /// </summary>
        /// <value>The admin password.</value>
        private string AdminPassword { get; set; }

        /// <summary>
        /// Gets or sets the target username.
        /// </summary>
        /// <value>The target username.</value>
        private string TargetUsername { get; set; }

        /// <summary>
        /// Gets the session SHA.
        /// Session SHA is encrypted with the target username (when request is done on the behalf of another user) 
        /// as well as a value representing the validity duration of this token (default is 3600s=1h).
        /// If no target username set request is done for the admin username.
        /// </summary>
        /// <value>The session SHA.</value>
        protected string SessionSHA { 
            get {
                if(string.IsNullOrWhiteSpace(this.TargetUsername) || this.TargetUsername.Equals(this.AdminUsername))
                {
                    return $"{this.AdminUsername}:{this.AdminPassword}";
                }

                return $"{this.AdminUsername}:{this.AdminPassword}:{this.TargetUsername}";
            } 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenNebula.OneClient"/> class.
        /// </summary>
        /// <param name="proxy">Proxy url of the XML RPC server</param>
        /// <param name="adminUsername">Admin username.</param>
        /// <param name="adminPassword">Admin password.</param>
        public OneClient(string proxy, string adminUsername, string adminPassword) {
            if (proxy == null) throw new Exception("ONe XML RPC proxy url cannot be null");
            if (adminUsername == null) throw new Exception("ONe XML RPC user cannot be null");

            this.ProxyUrl = proxy;
            this.AdminUsername = adminUsername;
            this.TargetUsername = adminUsername;
            this.AdminPassword = adminPassword;
        }
            
        /// <summary>
        /// Creates the proxy management object
        /// </summary>
        /// <returns>The proxy.</returns>
        /// <param name="type">Type.</param>
        public IXmlRpcProxy GetProxy(Type type){
            MethodInfo mi = typeof(XmlRpcProxyGen).GetMethod("Create", new Type[]{});
            MethodInfo gmi = mi.MakeGenericMethod(type);
            IXmlRpcProxy result = (IXmlRpcProxy)gmi.Invoke(null,null);
            result.Url = this.ProxyUrl;
            return result;
        }

        /// <summary>
        /// Starts delegating requests on the behalh of another user
        /// </summary>
        /// <param name="username">Name of the target user.</param>
        public void StartDelegate(string username){
            this.TargetUsername = username;
        }

        /// <summary>
        /// Ends delegating requests on the behalh of another user
        /// </summary>
        public void EndDelegate(){
            this.TargetUsername = this.AdminUsername;
        }

        /// <summary>
        /// Deserializes the response.
        /// </summary>
        /// <returns>The response.</returns>
        /// <param name="type">Type.</param>
        /// <param name="response">Response.</param>
        private object Deserialize(Type type, string response){
            object result = null;

            try{
                System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(type);
                using (System.IO.MemoryStream s = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(response ?? ""))) {
                    result = ser.Deserialize(s);
                }
            }catch(Exception e){
                throw new Exception(response);
            }

            return result;
        }

    }
}

