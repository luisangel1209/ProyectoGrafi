using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class infoPartida
{
    public static bool hayPartidaGuardada = false;

    public static class infoPersonaje{
        public static Vector2 posicion;
        public static int energiaActual;
        public static int cantBalas;
    }

    //para guardar las balas
    public class TipoInfoPaqueteBalas{
        public bool activo;
    }

    public static List<TipoInfoPaqueteBalas> infoPaqueteBalas = new List<TipoInfoPaqueteBalas>();

    //para guardar los zombies
    public class TipoInfoZombies{
        public bool activo;
        public Vector2 posicion;
    }

    public static List<TipoInfoZombies> infoZombies = new List<TipoInfoZombies>();
}
