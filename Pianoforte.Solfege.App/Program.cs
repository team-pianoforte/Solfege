﻿using System;
namespace Pianoforte.Solfege.Env.Desktop
{
  public static class Program
  {


    [STAThread]
    static void Main(string[] args)
    {
      using var game = new SenaGame(args[0]);
      game.Run();
    }
  }
}
