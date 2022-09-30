using UnityEngine;
using Zenject;

namespace Hellmade
{
    public class SoundManagerInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<EazySoundManager>().FromComponentOn(CreateSoundManager()).AsSingle().NonLazy(); 
        }

        private static GameObject CreateSoundManager()
        {
            var soundManager = new GameObject("EasySoundManager").AddComponent<EazySoundManager>();
            soundManager.Init();
            return soundManager.gameObject;
        }
    }
}