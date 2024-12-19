﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ItemColors.xaml.cs
 * PURPOSE:     UserControl, that holds the ItemColor controls
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using ViewModel;

namespace Debugger
{
    internal sealed class ItemColorsView
    {
        /// <summary>
        ///     Gets the delete command.
        /// </summary>
        /// <value>
        ///     The delete command.
        /// </value>
        public ICommand AddCommand =>
            new DelegateCommand<object>(AddAction, CanExecute);

        /// <summary>
        ///     Gets or sets the reference.
        /// </summary>
        /// <value>
        ///     The reference.
        /// </value>
        public ItemColors Reference { get; internal set; }

        /// <summary>
        ///     Gets or sets the filter.
        /// </summary>
        /// <value>
        ///     The filter.
        /// </value>
        public Dictionary<int, ItemColor> Filter { get; internal set; }

        /// <summary>
        ///     Adds the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void AddAction(object obj)
        {
            Reference.AddFilter();
        }

        /// <inheritdoc cref="PropertyChangedEventHandler" />
        /// <summary>
        ///     Triggers if an Attribute gets changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets a value indicating whether this instance can execute.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can execute the specified object; otherwise, <c>false</c>.
        /// </returns>
        /// <value>
        ///     <c>true</c> if this instance can execute; otherwise, <c>false</c>.
        /// </value>
        public bool CanExecute(object obj)
        {
            // check if executing is allowed, not used right now
            return true;
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        ///     Done action.
        /// </summary>
        /// <returns>All Color Options that were generated.</returns>
        public List<ColorOption> GetOption()
        {
            var options = new List<ColorOption>(Filter.Count);
            options.AddRange(Filter.Values.Select(filter => filter.View.Options));

            return options;
        }
    }
}
