/*
*	Copyright (c) 2017 Antonio Luevano. All rights reserved. 
* 
* This code is licensed under the MIT License (MIT). 
*
* Permission is hereby granted, free of charge, to any person obtaining a copy 
* of this software and associated documentation files (the "Software"), to deal 
* in the Software without restriction, including without limitation the rights 
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
* of the Software, and to permit persons to whom the Software is furnished to do 
* so, subject to the following conditions: 

* The above copyright notice and this permission notice shall be included in all 
* copies or substantial portions of the Software. 
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
* THE SOFTWARE. 
*/

using System;

namespace IdentityServer.Core.Events
{
    /// <summary>
    /// Models base class for events raised from IdentityServer.
    /// </summary>
    public class Event<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Event{T}" /> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException">category</exception>
        public Event(string category, string name, EventTypes type, int id, string message = null)
        {
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException("category");

            Category = category;
            Name = name;
            EventType = type;
            Id = id;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Event{T}" /> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="details">The details.</param>
        /// <param name="message">The message.</param>
        public Event(string category, string name, EventTypes type, int id, T details, string message = null)
            : this(category, name, type, id, message)
        {
            Details = details;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Event{T}" /> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="detailsFunc">The details function.</param>
        /// <param name="message">The message.</param>
        public Event(string category, string name, EventTypes type, int id, Func<T> detailsFunc, string message = null)
            : this(category, name, type, id, message)
        {
            DetailsFunc = detailsFunc;
        }

        /// <summary>
        /// Gets or sets the details function.
        /// </summary>
        /// <value>
        /// The details function.
        /// </value>
        [Newtonsoft.Json.JsonIgnore]
        public Func<T> DetailsFunc { get; set; }

        /// <summary>
        /// Allows event to defer data initialization until the event will be raised.
        /// </summary>
        internal void Prepare()
        {
            if (DetailsFunc != null)
            {
                Details = DetailsFunc();
            }
        }

        /// <summary>
        /// Gets or sets the event category. <see cref="EventConstants.Categories"/> for a list of the defined categories.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public EventTypes EventType { get; set; }

        /// <summary>
        /// Gets or sets the event identifier. <see cref="EventConstants.Ids"/> for the list of the defined identifiers.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the event message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the event details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public T Details { get; set; }

        /// <summary>
        /// Gets or sets the event context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public EventContext Context { get; set; }
    }
}