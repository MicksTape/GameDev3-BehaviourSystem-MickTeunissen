using UnityEngine;

namespace BTExample
{
    
    public class SomeAI : MonoBehaviour
    {
        private BTNode tree;
        public void Start()
        {
            tree =
                new BTSequence(
                    new BTWait(2f),
                    new BTDebug("Hoi")
                );
        }

        private void Update()
        {
            tree?.OnUpdate();
        }
    }
    
    public abstract class BTNode
    {
        public enum BTResult { Success, Failed, Running }
        public abstract BTResult Run();
        private bool isInitialized = false;

        public BTResult OnUpdate()
        {
            if (!isInitialized)
            {
                OnEnter();
                isInitialized = true;
            }
            BTResult result = Run();
            if(result != BTResult.Running)
            {
                OnExit();
                isInitialized = false;
            }
            return result;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
    }

    //Composite node
    public class BTSequence : BTNode
    {
        private BTNode[] children;
        private int currentIndex = 0;

        public BTSequence(params BTNode[] _children)
        {
            children = _children;
        }

        public override BTResult Run()
        {
            for (; currentIndex < children.Length; currentIndex++)
            {
                BTResult result = children[currentIndex].OnUpdate();
                switch (result)
                {
                    case BTResult.Failed:
                        currentIndex = 0;
                        return BTResult.Failed;
                    case BTResult.Running:
                        return BTResult.Running;
                    case BTResult.Success: break;
                }
            }
            currentIndex = 0;
            return BTResult.Success;
        }
    }

    //Composite node
    public class BTSelector : BTNode
    {
        private BTNode[] children;
        private int currentIndex = 0;

        public BTSelector(params BTNode[] _children)
        {
            children = _children;
        }

        public override BTResult Run()
        {
            for (; currentIndex < children.Length; currentIndex++)
            {
                BTResult result = children[currentIndex].OnUpdate();
                switch (result)
                {
                    case BTResult.Failed: break;
                    case BTResult.Running:
                        return BTResult.Running;
                    case BTResult.Success:
                        currentIndex = 0;
                        return BTResult.Success;
                }
            }
            currentIndex = 0;
            return BTResult.Failed;
        }
    }

    //Action node
    public class BTWait : BTNode
    {
        private float waitTime;
        private float currentTime;
        public BTWait(float _waitTime)
        {
            waitTime = _waitTime;
        }

        public override BTResult Run()
        {
            currentTime += Time.deltaTime;
            if (currentTime >= waitTime)
            {
                currentTime = 0;
                return BTResult.Success;
            }
            return BTResult.Running;

        }
    }

    //Action node
    public class BTDebug : BTNode
    {
        private string debugMessage;
        public BTDebug(string _debugMessage)
        {
            debugMessage = _debugMessage;
        }

        public override BTResult Run()
        {
            Debug.Log(debugMessage);
            return BTResult.Success;
        }
    }
}

