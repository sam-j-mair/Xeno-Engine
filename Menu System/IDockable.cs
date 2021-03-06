﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace XenoEngine.Systems.MenuSystem
{
    /// <summary>
    /// interface for implementing an dockable gui object
    /// </summary>
   public interface IDockable : INotifyPropertyChanged
   {
       Vector2 Position { get; set; }
       float Width { get; }
       float Height { get; }
    }
}
