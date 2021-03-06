using UnityEngine;
using System.Collections.Generic;



public class CachedFlatParticles
{
	List<FlatElementBase> mParticles = new List<FlatElementBase>();
	
	public bool has_particle()
	{
		return mParticles.Count > 0;
	}
	
	public FlatElementBase take_particle()
	{
		FlatElementBase r = null;
		if(has_particle())
		{
			r = mParticles[mParticles.Count - 1];
			r.Enabled = true;
			mParticles.RemoveAt(mParticles.Count - 1);
		}
		return r;
	}
	
	public void return_particle(FlatElementBase aParticle)
	{
		aParticle.Enabled = false;
		mParticles.Add(aParticle);
	}
}


//man this class is really really dumb..
public class SparkleStarFlashParticle
{
	
	FlatParticleEmitter mEmitter;
	FlatParticleEmitter mEmitter2;
	FlatParticleEmitter mEmitter3;  //use this one for sky particles
	Dictionary<string, CachedFlatParticles> mCachedParticles = new Dictionary<string, CachedFlatParticles>();
	public string[] mParticleTypes = new string[]{"gold","silver","glow","red"};

	public void set_emitter_particle_color(Color aColor, int emitterNum)
	{
		Color endColor = aColor;
		endColor.a = 0;
		if(emitterNum == 1)
		{
			mEmitter.StartColor = aColor;
			mEmitter.EndColor = endColor;
		}
		else if(emitterNum == 2)
		{
			mEmitter2.StartColor = aColor;
			mEmitter2.EndColor = endColor;
		}
		else if(emitterNum == 3)
		{
			mEmitter3.StartColor = aColor;
			mEmitter3.EndColor = endColor;
		}


	}
	
	public SparkleStarFlashParticle()
	{
		mEmitter = new FlatParticleEmitter()
		{
			UseColor = true,
			StartColor = GameConstants.UiWhite,
			EndColor = GameConstants.UiWhiteTransparent
		};
		
		mEmitter2 = new FlatParticleEmitter()
		{
			UseColor = true,
			StartColor = GameConstants.UiWhite,
			EndColor = GameConstants.UiWhiteTransparent
		};

		mEmitter3 = new FlatParticleEmitter()
		{
			UseColor = true,
			StartColor = GameConstants.UiWhite,
			EndColor = GameConstants.UiWhiteTransparent,
			Gravity = new Vector3(0,-10000,0)
		};
		
		foreach(string e in mParticleTypes)
		{
			mCachedParticles[e] = new CachedFlatParticles();
		}
	}
	
	public void create_cached_particles()
	{
		//TODO
	}
	
	public void emit_point(int count, Vector3 position, float speed)
	{
		emit_ring("gold",count,position,speed,1.3f);
	}

	public void emit_ring(string type, int count, Vector3 position, float speed, float scale)
	{
		for(int i = 0; i < count; i++)
			create_particle(type,
			                position,
			                speed*new Vector3(Mathf.Cos (i/(float)count*Mathf.PI*2),Mathf.Sin (i/(float)count*Mathf.PI*2),0),
			                new QuTimer(-0.5f,0.2f),
			                scale,
			                true
			                );
	}

	public void emit_line(string type, int count, Vector3 lineStart, Vector3 lineEnd, float scale)
	{
		for(int i = 0; i < count; i++)
		{
			float lambda = Random.Range(0f,1f);
			create_particle(type,
			                lineStart*lambda + lineEnd*(1-lambda),
			                Vector3.zero,
			                new QuTimer(-1f,0.2f),
			                scale,
			                true
			                );
		}
	}

	public void emit_continuous(float grade, Vector3 position)
	{
		float ag = grade*grade;
		
		if(grade > 0.5f)
			for(int i = 0; i < 4*ag; i++)
				if(Random.value < 0.0075f)
					create_particle("silver",position,Random.insideUnitCircle*500);
	}

	public void create_particle(string aType, Vector3 position, Vector3 speed, QuTimer aTimer, float aScale, bool aUseRandomRotation)
	{
		var cache = mCachedParticles[aType];
		if(!cache.has_particle())
		{
			Texture2D tex = null;
			if(aType == "gold")
				tex = ManagerManager.Manager.mNewRef.partGold;
			if(aType == "silver")
				tex = ManagerManager.Manager.mNewRef.partSilver2;
			var newPart = new FlatElementImage(tex,new Vector2(60,60),100);
			foreach (Renderer f in newPart.PrimaryGameObject.GetComponentsInChildren<Renderer>())	
				f.gameObject.layer = ManagerManager.Manager.mBackgroundManager.mBackgroundLayer;
			cache.return_particle(newPart);
		}
		var emitter = aType == "gold" ? mEmitter2 : mEmitter;
		var part = emitter.add_particle(cache.take_particle(),position,speed,1f,aType);
		part.element.HardFlatRotation = Random.Range(0f,360f);
		part.element.HardScale = aScale * Vector3.one;
		part.timer = new QuTimer(aTimer.getCurrent(),aTimer.getTarget());
	}

	public void create_particle(string aType, Vector3 position, Vector3 speed, int emitterNum = 1)
	{
		//TODO diff particle types
		var cache = mCachedParticles[aType];
		if(!cache.has_particle())
		{
			Texture2D tex = null;
			if(aType == "gold")
				tex = ManagerManager.Manager.mNewRef.partGold;
			if(aType == "silver")
				tex = ManagerManager.Manager.mNewRef.partSilver2;
			var newPart = new FlatElementImage(tex,new Vector2(60,60),5);
			if(aType == "silver")
			{
				newPart.HardScale = 1.2f*(new Vector3(1,1,1));
				newPart.HardFlatRotation = Random.Range(0f,360f);
			}
			if(aType == "gold")
				newPart.HardScale = 1.7f*(new Vector3(1,1,1));
			foreach (Renderer f in newPart.PrimaryGameObject.GetComponentsInChildren<Renderer>())	
				f.gameObject.layer = ManagerManager.Manager.mBackgroundManager.mBackgroundLayer;
			cache.return_particle(newPart);
		}

		var part = mEmitter.add_particle(cache.take_particle(),position,speed,0.7f,aType);
		if(aType == "silver")
		{
			part.timer = new QuTimer(-0.5f,0.2f);
			
		}
		
	}
	
	public void update(float aDelta)
	{
		foreach(var e in mEmitter.update_particles(aDelta))
		{
			mCachedParticles[e.id].return_particle(e.element);
		}
		foreach(var e in mEmitter2.update_particles(aDelta))
		{
			mCachedParticles[e.id].return_particle(e.element);
		}
		foreach(var e in mEmitter3.update_particles(aDelta))
		{
			mCachedParticles[e.id].return_particle(e.element);
		}
	}
	
	
	//point particles 
	//0.9-1 few Golden stars, large
	//0.8-0.9 few golden stars small
	//0.7-0.8 many silver stars + flashy oriented lines
	//0.6-0.7 silver stars
	//0.5-0.6 silver sparks
	//0.3-0.5 red glow??
	//0.0-0.3 red sparks
	
	
	
	
	//TODO move this somewhere else
    static Dictionary<Color, Texture2D> mColorTextures = new Dictionary<Color, Texture2D>();
    public static Texture2D get_color_texture(Color aColor)
    {
        if (!mColorTextures.ContainsKey(aColor))
        {
            int w = 2;
            int h = 2;
            Texture2D rgb_texture = new Texture2D(w, h);
            Color rgb_color = aColor;
            int i, j;
            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    rgb_texture.SetPixel(i, j, rgb_color);
                }
            }
            rgb_texture.Apply();
            mColorTextures[aColor] = rgb_texture;
        }
        return mColorTextures[aColor];
    }
}


//this doesn't actually use anything from flat other than position...
public class FlatParticleEmitter : FlatElementBase
{
	public class FlatSubParticle
	{
		public string id;
		public Vector3 pos;
		public Vector3 vel;
		public QuTimer timer;
		public FlatElementBase element;
	}
	
	
	LinkedList<FlatSubParticle> mParticles = new LinkedList<FlatSubParticle>();
	
	public FlatParticleEmitter()
	{
		PrimaryGameObject = new GameObject("genDummyFlatPraticleEmitter");
	}
	
	public void initialize_defaults()
	{
		Gravity = new Vector3(0,0,0);
		UseColor = true;
		StartColor = GameConstants.UiWhite;
		EndColor = GameConstants.UiWhite;
	}
	
	public FlatSubParticle add_particle(FlatElementBase aParticle, Vector3 aPos, Vector3 aVel, float aLifetime, string aId)
	{
		FlatSubParticle addMe = new FlatSubParticle();
		addMe.id = aId;
		addMe.pos = aPos;
		addMe.vel = aVel;
		addMe.element = aParticle;
		addMe.timer = new QuTimer(0,aLifetime);
		mParticles.AddLast(addMe);
		return addMe;
	}
	
	public Vector3 Gravity {get; set;}
	
	public bool UseColor {get; set;}
	public Color StartColor {get; set;}
	public Color EndColor {get; set;}
	
	
	public List<FlatSubParticle> update_particles(float aDelta)
	{
		List<FlatSubParticle> removed = new List<FlatSubParticle>();
		LinkedListNode<FlatSubParticle> starting = mParticles.First;
		while(starting != null)
		{
			LinkedListNode<FlatSubParticle> operating = starting;
			FlatSubParticle e = starting.Value;
			e.pos += e.vel*aDelta;
			e.vel += Gravity*aDelta;
			e.timer.update(aDelta);
			starting = starting.Next;
			if(e.timer.isExpired())
			{
				mParticles.Remove(operating);
				removed.Add(operating.Value);
			}
			
			//update the element
			e.element.HardPosition = e.pos;
			if(UseColor)
			{
				float lambda = Mathf.Clamp01(e.timer.getLinear());
				e.element.HardColor = StartColor*(1-lambda) + EndColor*lambda;
			}
			
			e.element.update(aDelta);
		}
		return removed;
	}
}
