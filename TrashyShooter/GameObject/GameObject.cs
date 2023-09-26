using System.Reflection;


namespace MultiplayerEngine
{
    /// <summary>
    /// an object that can hold information and update information using given components
    /// Niels
    /// </summary>
    public class GameObject
    {
        #region Variables & Fields
        /// <summary>
        /// A list of the componets on the object
        /// </summary>
        private List<Component> _components = new List<Component>();
        public List<Component> Components { get { return _components; } }
        /// <summary>
        /// the objects transform
        /// </summary>
        public Transform transform { get; set; } = new Transform();
        /// <summary>
        /// the name of the gameobject
        /// </summary>
        public string name = "gameobject";
        /// <summary>
        /// is the object active or not
        /// </summary>
        public bool enabled = true;
        #endregion

        #region Constructer
        /// <summary>
        /// intatiates an empty gameobject and adds it to the open scene
        /// </summary>
        public GameObject()
        {
            SceneManager.active_scene.gameObjects.Add(this);
        }
        #endregion

        #region Methods
        /// <summary>
        /// adds a chosen component to the gameobject and returns the created component
        /// </summary>
        /// <typeparam name="T">the component to add</typeparam>
        /// <returns>the added component</returns>
        public T AddComponent<T>() where T : Component
        {
            // Get the constructor of component
            Type type = typeof(T);
            ConstructorInfo parameterConstructor = type.GetConstructor(new Type[] { typeof(GameObject) });

            // If component has a constructor with a GameObject constructor, use it and pass this as parameter
            Component component = parameterConstructor != null
                ? (Component)Activator.CreateInstance(typeof(T), this)
                : (Component)Activator.CreateInstance(typeof(T), true);

            _components.Add(component);
            component.gameObject = this;
            component.transform = transform;
            return (T)component;
        }

        /// <summary>
        /// gets a component of chosen type
        /// </summary>
        /// <typeparam name="T">the component type to find</typeparam>
        /// <returns>the component</returns>
        public T GetComponent<T>() where T : Component
        {
            return (T)_components.Find(x => x.GetType() == typeof(T));
        }

        /// <summary>
        /// checks if the gameobject has a given component
        /// </summary>
        /// <typeparam name="T">component to check for</typeparam>
        /// <returns>boolean based on if the gameobject has the component</returns>
        public bool HasComponent<T>() where T : Component
        {
            Component component = _components.Find(x => x.GetType() == typeof(T));

            return component != null;
        }
        #endregion

        #region Standard Metods
        /// <summary>
        /// called when gameobject is intantiated
        /// </summary>
        public void Awake()
        {
            InvokeComponentsMethod("Awake", null);
        }

        /// <summary>
        /// gets called the first frame the gameobject is active
        /// </summary>
        public void Start()
        {
            InvokeComponentsMethod("Start", null);
        }

        /// <summary>
        /// Gets called every frame
        /// </summary>
        /// <param name="gameTime">the time that the past frame took</param>
        public void Update(GameTime gameTime)
        {
            InvokeComponentsMethod("Update", null);
        }

        /// <summary>
        /// gets called at the end of each frame and calls all componts draw function
        /// </summary>
        /// <param name="spriteBatch">the games sprite batch</param>
        public void Draw3D()
        {
            InvokeComponentsMethod("Draw3D", new object[] { });
        }

        /// <summary>
        /// gets called at the end of each frame and calls all componts draw function
        /// </summary>
        /// <param name="spriteBatch">the games sprite batch</param>
        public void Draw2D(SpriteBatch spriteBatch)
        {
            InvokeComponentsMethod("Draw2D", new object[] { spriteBatch });
        }

        /// <summary>
        /// gets called at the end of each frame and calls all componts draw function
        /// </summary>
        /// <param name="spriteBatch">the games sprite batch</param>
        public void DrawUI(SpriteBatch spriteBatch)
        {
            InvokeComponentsMethod("DrawUI", new object[] { spriteBatch });
        }

        public void OnCollision(GameObject go)
        {
            InvokeComponentsMethod("OnCollision", new object[] { go });
        }

        public void StopSound()
        {
            InvokeComponentsMethod("StopSound", new object[] {  });
        }

        /// <summary>
        /// invokes a method in all components on tyhe gameobject if the method is implemented
        /// </summary>
        /// <param name="methodName">the name of the method</param>
        /// <param name="parameters">the paremeters the method needs</param>
        private void InvokeComponentsMethod(string methodName, object[] parameters)
        {
            for (int i = 0; i < _components.Count; ++i)
            {
                if (_components[i] == null)
                    continue;
                if (!_components[i].enabled)
                    continue;

                Type componentType = _components[i].GetType();
                MethodInfo method = componentType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(_components[i], parameters);
            }
        }
        #endregion
    }
}
