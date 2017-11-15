// Copyright (c) 2017 Kastellanos Nikolaos

using System;
using System.Collections.Generic;

namespace tainicom.Aether.Physics2D.Dynamics.Contacts
{
    /// <summary>
    /// Head of a circular doubly linked list.
    /// </summary>
    public class ContactListHead : Contact
    {
        internal ContactListHead(): base(null, 0, null, 0)
        {
            this.Prev = this;
            this.Next = this;
        }
    }
}
