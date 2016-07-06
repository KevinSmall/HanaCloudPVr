using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class ExtensionMethods
{
   /// <summary>
   /// Returns the minimal element of the given sequence, based on
   /// the given projection.
   /// </summary>
   /// <remarks>
   /// If more than one element has the minimal projected value, the first
   /// one encountered will be returned. This overload uses the default comparer
   /// for the projected type. This operator uses immediate execution, but
   /// only buffers a single result (the current minimal element).
   /// </remarks>
   /// <typeparam name="TSource">Type of the source sequence</typeparam>
   /// <typeparam name="TKey">Type of the projected element</typeparam>
   /// <param name="source">Source sequence</param>
   /// <param name="selector">Selector to use to pick the results to compare</param>
   /// <returns>The minimal element, according to the projection.</returns>
   /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
   /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

   public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
       Func<TSource, TKey> selector)
   {
      return source.MinBy(selector, null);
   }

   /// <summary>
   /// Returns the minimal element of the given sequence, based on
   /// the given projection and the specified comparer for projected values.
   /// </summary>
   /// <remarks>
   /// If more than one element has the minimal projected value, the first
   /// one encountered will be returned. This operator uses immediate execution, but
   /// only buffers a single result (the current minimal element).
   /// </remarks>
   /// <typeparam name="TSource">Type of the source sequence</typeparam>
   /// <typeparam name="TKey">Type of the projected element</typeparam>
   /// <param name="source">Source sequence</param>
   /// <param name="selector">Selector to use to pick the results to compare</param>
   /// <param name="comparer">Comparer to use to compare projected values</param>
   /// <returns>The minimal element, according to the projection.</returns>
   /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
   /// or <paramref name="comparer"/> is null</exception>
   /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

   public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
       Func<TSource, TKey> selector, IComparer<TKey> comparer)
   {
      if (source == null) throw new ArgumentNullException("source");
      if (selector == null) throw new ArgumentNullException("selector");
      comparer = comparer ?? Comparer<TKey>.Default;

      using (var sourceIterator = source.GetEnumerator())
      {
         if (!sourceIterator.MoveNext())
         {
            throw new InvalidOperationException("Sequence contains no elements");
         }
         var min = sourceIterator.Current;
         var minKey = selector(min);
         while (sourceIterator.MoveNext())
         {
            var candidate = sourceIterator.Current;
            var candidateProjected = selector(candidate);
            if (comparer.Compare(candidateProjected, minKey) < 0)
            {
               min = candidate;
               minKey = candidateProjected;
            }
         }
         return min;
      }
   }

   /// <summary>
   /// Returns the maximal element of the given sequence, based on
   /// the given projection.
   /// </summary>
   /// <remarks>
   /// If more than one element has the maximal projected value, the first
   /// one encountered will be returned. This overload uses the default comparer
   /// for the projected type. This operator uses immediate execution, but
   /// only buffers a single result (the current maximal element).
   /// </remarks>
   /// <typeparam name="TSource">Type of the source sequence</typeparam>
   /// <typeparam name="TKey">Type of the projected element</typeparam>
   /// <param name="source">Source sequence</param>
   /// <param name="selector">Selector to use to pick the results to compare</param>
   /// <returns>The maximal element, according to the projection.</returns>
   /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
   /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

   public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
       Func<TSource, TKey> selector)
   {
      return source.MaxBy(selector, null);
   }

   /// <summary>
   /// Returns the maximal element of the given sequence, based on
   /// the given projection and the specified comparer for projected values. 
   /// </summary>
   /// <remarks>
   /// If more than one element has the maximal projected value, the first
   /// one encountered will be returned. This operator uses immediate execution, but
   /// only buffers a single result (the current maximal element).
   /// </remarks>
   /// <typeparam name="TSource">Type of the source sequence</typeparam>
   /// <typeparam name="TKey">Type of the projected element</typeparam>
   /// <param name="source">Source sequence</param>
   /// <param name="selector">Selector to use to pick the results to compare</param>
   /// <param name="comparer">Comparer to use to compare projected values</param>
   /// <returns>The maximal element, according to the projection.</returns>
   /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
   /// or <paramref name="comparer"/> is null</exception>
   /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

   public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
       Func<TSource, TKey> selector, IComparer<TKey> comparer)
   {
      if (source == null) throw new ArgumentNullException("source");
      if (selector == null) throw new ArgumentNullException("selector");
      comparer = comparer ?? Comparer<TKey>.Default;

      using (var sourceIterator = source.GetEnumerator())
      {
         if (!sourceIterator.MoveNext())
         {
            throw new InvalidOperationException("Sequence contains no elements");
         }
         var max = sourceIterator.Current;
         var maxKey = selector(max);
         while (sourceIterator.MoveNext())
         {
            var candidate = sourceIterator.Current;
            var candidateProjected = selector(candidate);
            if (comparer.Compare(candidateProjected, maxKey) > 0)
            {
               max = candidate;
               maxKey = candidateProjected;
            }
         }
         return max;
      }
   }

   public static void PlayerPrefsSetBool(string name, bool booleanValue)
   //public static void SetBool(this PlayerPrefs p, string name, bool booleanValue)
   // above doesnt work as extensio methods need object instance
   {
      PlayerPrefs.SetInt(name, booleanValue ? 1 : 0);
   }

   public static bool PlayerPrefsGetBool(string name)
   {
      return PlayerPrefs.GetInt(name) == 1 ? true : false;
   }

   /// <summary>
   /// Returns the largest Rect of given aspect ratio that can be selected from within the calling Rect.
   /// Calling rectangle must be at position 0,0 we only use its width and height
   /// </summary>
   /// <param name="sourceRect">Source rectangle</param>
   /// <param name="desiredAspectRatio">The aspect ratio of the rectangle to return</param>
   /// <param name="squashyFactor">Use 1f for no squashing. Allow squashing eg 1.3 would allow 30% squashing or stretching</param>
   /// <returns>Largest Rect of given aspect ratio that can be selected from within calling Rect sourceRect</returns>
   public static Rect GetLargestRectangle(this Rect sourceRect, float desiredAspectRatio, float squashyFactor)
   {
      Rect desiredRectangle = new Rect();

      float sourceAspectRatio = (float)sourceRect.width / (float)sourceRect.height;
      if (desiredAspectRatio < sourceAspectRatio)
      {
         // Height can be full height, we lose a bit of the left and right edges
         desiredRectangle.height = sourceRect.height;
         desiredRectangle.y = 0;
         // Width
         desiredRectangle.width = (int)((float)sourceRect.height * desiredAspectRatio * squashyFactor);
         // Clamp width
         if (desiredRectangle.width > sourceRect.width)
         {
            desiredRectangle.width = sourceRect.width;
         }
         // Offset
         desiredRectangle.x = (int)(0.5f * (sourceRect.width - desiredRectangle.width));
         if (desiredRectangle.x < 0)
         {
            desiredRectangle.x = 0;
         }
      }
      else
      {
         // Width can be full width, we lose a bit of the top and bottom
         desiredRectangle.width = sourceRect.width;
         desiredRectangle.x = 0;
         // Height
         desiredRectangle.height = (int)((float)sourceRect.width / desiredAspectRatio * squashyFactor);
         // Clamp height
         if (desiredRectangle.height > sourceRect.height)
         {
            desiredRectangle.height = sourceRect.height;
         }
         // Offset
         desiredRectangle.y = (int)(0.5f * (sourceRect.height - desiredRectangle.height));
         if (desiredRectangle.y < 0)
         {
            desiredRectangle.y = 0;
         }
      }

      return desiredRectangle;
   }
}