using UnityEngine;

namespace AnyRPG {

    public class TerrainDetector {
        private TerrainData terrainData;
        private int alphamapWidth;
        private int alphamapHeight;
        private float[,,] splatmapData;
        private int numTextures;

        public void LoadSceneSettings() {
            //Debug.Log("TerrainDetector.LoadSceneSettings()");
            if (Terrain.activeTerrain == null) {
                ClearSceneSettings();
                return;
            }
            terrainData = Terrain.activeTerrain.terrainData;
            alphamapWidth = terrainData.alphamapWidth;
            alphamapHeight = terrainData.alphamapHeight;

            splatmapData = terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
            numTextures = splatmapData.Length / (alphamapWidth * alphamapHeight);
            //Debug.Log($"TerrainDetector.LoadSceneSettings(); numTextures: {numTextures}");
        }

        public void ClearSceneSettings() {
            terrainData = null;
            alphamapWidth = 0;
            alphamapHeight = 0;

            splatmapData = new float[,,] { };
            numTextures = 0;
        }

        private Vector3 ConvertToSplatMapCoordinate(Vector3 worldPosition) {
            //Debug.Log($"TerrainDetector.ConvertToSplatMapCoordinate({worldPosition})");
            Vector3 splatPosition = new Vector3();
            Terrain ter = Terrain.activeTerrain;
            Vector3 terPosition = ter.transform.position;
            splatPosition.x = ((worldPosition.x - terPosition.x) / ter.terrainData.size.x) * ter.terrainData.alphamapWidth;
            splatPosition.z = ((worldPosition.z - terPosition.z) / ter.terrainData.size.z) * ter.terrainData.alphamapHeight;

            //Debug.Log($"TerrainDetector.ConvertToSplatMapCoordinate({worldPosition}); return {splatPosition}");
            return splatPosition;
        }

        public int GetActiveTerrainTextureIdx(Vector3 position) {
            //Debug.Log($"TerrainDetector.GetActiveTerrainTextureIdx({position})");
            Vector3 terrainCord = ConvertToSplatMapCoordinate(position);
            int activeTerrainIndex = 0;
            float largestOpacity = 0f;

            for (int i = 0; i < numTextures; i++) {
                if (largestOpacity < splatmapData[(int)terrainCord.z, (int)terrainCord.x, i]) {
                    activeTerrainIndex = i;
                    largestOpacity = splatmapData[(int)terrainCord.z, (int)terrainCord.x, i];
                }
            }

            //Debug.Log($"TerrainDetector.GetActiveTerrainTextureIdx({position}); return {activeTerrainIndex}");
            return activeTerrainIndex;
        }

    }
}