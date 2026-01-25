using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static T Draw<T>(this List<T> list)
    {
        if (list.Count == 0) return default;
        int random = Random.Range(0, list.Count);
        T t = list[random];
        list.Remove(t);
        return t;
    }
}
