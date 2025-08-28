using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroExpSquare : MonoBehaviour
{ 
  public enum ExpType
  {
      GreenHero,
      YellowHero,
      BlueHero,
      Any
  }
  
  [Serializable]
  public struct EXPSprite
  {
      public ExpType expType;
      public Sprite expSprite;
  }
 
  public EXPSprite[] heroEXPSprites;
  
  private Dictionary<ExpType,Sprite> spriteDic = new Dictionary<ExpType, Sprite>();

  private SpriteRenderer sprite;
  public int NumHeroExp
  {
      get
      {
          return heroEXPSprites.Length;
      }
  }

  private ExpType curExpType;
  public ExpType CurExpType
  {
      get
      {
          return curExpType;
      }
      set
      {
          SetExp(value);
      }
  }
  
  private void Awake()
  {
      sprite=  transform.Find("HeroExpSprite").GetComponent<SpriteRenderer>();
      for (int i = 0; i < heroEXPSprites.Length; i++)
      {
          if (!spriteDic.ContainsKey(heroEXPSprites[i].expType))
          {
              spriteDic.Add(heroEXPSprites[i].expType, heroEXPSprites[i].expSprite);
          }
      }
  }

  public void SetExp(ExpType newExpType)
  {
      curExpType = newExpType;
      if (spriteDic.ContainsKey(newExpType))
      {
          sprite.sprite = spriteDic[newExpType];
      }
  }
}
