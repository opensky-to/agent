﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandEventArgs.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.MVVM
{
    using System;

    using JetBrains.Annotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// CommandEventArgs - simply holds the command parameter.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/03/2021.
    /// </remarks>
    /// <seealso cref="T:System.EventArgs"/>
    /// -------------------------------------------------------------------------------------------------
    public class CommandEventArgs : EventArgs
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the parameter.
        /// </summary>
        /// <value>
        /// The parameter.
        /// </value>
        /// -------------------------------------------------------------------------------------------------
        [CanBeNull]
        public object Parameter { get; set; }
    }
}