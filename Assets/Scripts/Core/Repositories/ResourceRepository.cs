using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace Core.Repositories
{
    [Serializable]
    public class Image
    {
        public string id;
        public Sprite sprite;
    }

    [Serializable]
    public class Image<TId>
    {
        public TId id;
        public Sprite sprite;
    }

    [Serializable]
    public class PrefabResource<T> where T : MonoBehaviour
    {
        public T data;
        public string id;
    }

    [CreateAssetMenu(fileName = "ResourceRepository", menuName = "Repositories/ResourceRepository")]
    public class ResourceRepository : ScriptableObject
    {
        [SerializeField] private Sprite defaultUnit;
        [SerializeField] private List<Image> extraUnitImages;
        [SerializeField] private List<Image> unitImages;

        public Sprite GetUnitImageById(string id, bool useDefault = true)
        {
            var unitImage = unitImages.GetBy(value => value.id == id);
            unitImage ??= extraUnitImages.GetBy(value => value.id == id);
            return unitImage == null ? useDefault ? defaultUnit : null : unitImage.sprite;
        }

        public void DownloadImages()
        {
#if UNITY_EDITOR
            DownloadToList(new[] {"Assets/Game/Units/Sprites/Units"}, unitImages);
            Debug.Log($"[{nameof(ResourceRepository)}] Update Success");
#endif
        }

        private void DownloadToList<T>(string[] folders, List<PrefabResource<T>> destination, bool clear = true)
            where T : MonoBehaviour
        {
#if UNITY_EDITOR
            if (clear)
                destination.Clear();

            var objects = new List<Object>();
            var guids = AssetDatabase.FindAssets("t:GameObject", folders);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                objects.Add(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
            }

            foreach (var obj in objects)
            {
                destination.Add(new PrefabResource<T> {id = obj.name, data = ((GameObject) obj).GetComponent<T>()});
            }
#endif
        }

        private void DownloadToList(string[] folders, List<Image> destination)
        {
#if UNITY_EDITOR
            destination.Clear();
            var sprites = new List<Sprite>();
            foreach (var folder in folders)
            {
                var spriteSheetPaths = Directory.GetFiles($"{folder}/", "*");
                foreach (var path in spriteSheetPaths)
                    sprites.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToList());
            }

            foreach (var sprite in sprites)
            {
                destination.Add(new Image {id = sprite.name, sprite = sprite});
            }
#endif
        }

        public List<Sprite> GetUnitSpritesByIds(List<string> unitIds)
        {
            var images = unitImages.GetListBy(value => unitIds.Contains(value.id));
            var result = new List<Sprite>();
            foreach (var image in images)
            {
                result.Add(image.sprite);
            }

            return result;
        }
    }
}