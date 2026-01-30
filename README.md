# VR Zen Garden - Japanese Garden Experience

A peaceful Virtual Reality experience set in a traditional Japanese Zen garden, built with Unity for Google Cardboard.


https://github.com/user-attachments/assets/e92ecd8c-76c5-4216-8530-cf8454de7836


## Description
This project is a mobile VR zen garden meant for calm exploration and simple interaction.

The player can walk around the garden using a look-to-walk movement system. Japanese gates are placed throughout the scene; when standing under one gate, the others become outlined. By holding a long gaze on a selected gate, the player can teleport to it.

Tilting the head or phone about 30 degrees to one side recenters the view, while tilting to the other side opens a small menu with options to pause or exit the experience.

Most interactions are gaze-based. A rake in the scene can be picked up with a long gaze. While holding it, walking over sand leaves visible trails. The rake can be dropped with a tap. There is also a house that the player can enter, with doors that open and close using a long gaze.

The environment includes several ambient elements. Walking into the gong triggers a sound. Lanterns can be turned on and off with a long gaze. A water basin and a koi pond provide spatial audio, and the koi pond contains animated fish. Bushes slow down movement when walked through and visually jiggle on contact.

To help reduce motion sickness, vignetting and a movement dead zone are implemented.

## About
This project creates an immersive VR environment featuring:
- Traditional Japanese garden aesthetics
- Koi ponds with swimming fish
- Stone lanterns, pagodas, and Torii gates
- Interactive elements (doors, gong, water basin)
- Realistic water and fire effects

## Important: This Repository Contains Code Only
This repository contains the **project code and structure** but **NOT the 3D assets, textures, or audio files** due to licensing restrictions. The `.meta` files are preserved to maintain Unity references, but you must obtain the actual assets separately.

## Required Assets
To build this project, you need to download and install the following assets:

### Unity Asset Store Packages
1. **#NVJOB Water Shaders V2** by Nick Veselov
   - [Asset Store Link](https://assetstore.unity.com/packages/vfx/shaders/nvjob-water-shaders-v2-149916)
   - Place in: `Assets/#NVJOB Water Shaders V2/`

2. **Yughues Free Bushes** by Nobiax / Yughues
   - Place in: `Assets/YughuesFreeBushes2018/`

3. **18 High Resolution Wall Textures** by A dog's life software
   - Place in: `Assets/ADG_Textures/`

4. **Japanese Garden 2 Free** by Waldemarst
   - Place in: `Assets/Waldemarst/`

5. **8K Skybox Pack Free** by BG Studio
   - Place in: `Assets/AllSkyFree/`
   - Alternative: **AllSky Free - 10 Sky / Skybox Set** by rpgwhitelock

6. **Japanese Zen Garden Pack** by Forge Creative
   - Place in: `Assets/Japanese Garden Pack/`

7. **Japanese Traditional Percussion** by Tobias Ungerboeck
   - Place in: `Assets/Japanese Traditional Percussion - One Adaptive Music Loop/`

8. **Shrine Torii Gate Pack** by Tanuki Digital
   - Place in: `Assets/JTS_Shrine_Torii_Gates/`

9. **Pagoda Architecture** by Tanuki Digital
   - Place in: `Assets/JTS_PagodaArchitecture/`

10. **Stone Lantern Pack** by Tanuki Digital
    - Place in: `Assets/JTS_StoneLanterns/`

11. **Shrine Pack** by logicalbeat
    - Includes Torii, Stone Lanterns, and more
    - Place in: `Assets/Shrine Pack/`

12. **Mossy Rocks - Photoscanned** by Mikołaj Spychał
    - Place in: `Assets/MossyRocks/`

13. **Outdoor Ground Textures** by A dog's life software
    - Place in: `Assets/GrassFlowers/`

14. **SUIMONO - WATER SYSTEM 2** (if used)
    - [Asset Store Link](https://assetstore.unity.com/packages/tools/particles-effects/suimono-water-system-2-38807)
    - Place in: `Assets/SUIMONO - WATER SYSTEM 2/`

### Sketchfab Models (CC Licensed)
Download these 3D models from Sketchfab (check individual licenses):

1. **Koi Fish** (Downloaded: 8.1.2026)
   - Source: https://sketchfab.com/3d-models/koi-fish-602b3cbfc4014fc3b890936c019ffb6f
   - Place in: `Assets/koi-fish/`
   - **Note:** Fish are rotated in script to swim forward (model's front ≠ Unity's forward)

2. **Gong** (Downloaded: 8.1.2026)
   - Source: https://sketchfab.com/3d-models/gong-30641a548633462187e889682361484b
   - Place in: `Assets/gong/`

3. **Lotus and Leaf** (Downloaded: 8.1.2026)
   - Source: https://sketchfab.com/3d-models/lotus-and-leaf-hoa-sen-tvc-24f835724f3348ec8f82549617c9cbdf
   - Place in: `Assets/lotus-and-leaf-hoa-sen-tvc/`

4. **Buddha Statue** (Downloaded: 8.1.2026)
   - Source: https://sketchfab.com/3d-models/buddha-statue-da6f055f8cc044728152b0dae4420cc3
   - Place in: `Assets/buddha-statue/`

5. **Tsukubai (Stone Water Basin)** (Downloaded: 8.1.2026)
   - Source: https://sketchfab.com/3d-models/tsukubai-japanese-stone-water-basin-cfa237d26f0f43a7b27dddd42048efff
   - Place in: `Assets/tsukubai-japanese-stone-water-basin/`

6. **Japanese Wood Bridge** (Downloaded: 8.1.2026)
   - Source: https://sketchfab.com/3d-models/japanese-wood-bridge-9d483d5e092544b38ee98abccce55249
   - Place in: `Assets/japanese-wood-bridge/`

7. **Tea Table Set** (Downloaded: 9.1.2026)
   - Source: https://sketchfab.com/3d-models/tea-table-set-36b31b3484df46a781409825a9903d00
   - Place in: `Assets/tea-table-set/`

8. **Geta (Japanese Traditional Shoes)** (Downloaded: 9.1.2026)
   - Source: https://sketchfab.com/3d-models/japanese-traditional-shoes-geta-5ddd5cafaebc4f3094d27c84dd8d643d
   - Place in: `Assets/flip-flops/`

### Polyhaven Textures (CC0 Licensed)

Download these PBR texture sets from Polyhaven:

1. **Sand 03** (Downloaded: 18.1.2026)
   - Source: https://polyhaven.com/a/sand_03
   - Place in: `Assets/sand_03_texture/`

2. **Rocky Terrain 02** (Downloaded: 23.1.2026)
   - Source: https://polyhaven.com/a/rocky_terrain_02
   - Used for grass/ground texture
   - Place in: `Assets/rocky_terrain_02_8k/`

### Audio Files (Freesound.org - CC Licensed)
Download these sound effects and place them in your project's audio folders:

1. **Footsteps on Sand** (Downloaded: 21.1.2026)
   - Source: https://freesound.org/people/FallujahQc/sounds/403169/
   - By: FallujahQc

2. **Door Opening** (Downloaded: 21.1.2026)
   - Source: https://freesound.org/people/JakLocke/sounds/261108/
   - By: JakLocke
   - **Note:** Door interaction required custom pivot point and script with precise coordinate adjustments

3. **Lake/Water Ambience** (Downloaded: 21.1.2026)
   - Source: https://freesound.org/people/afterguard/sounds/82487/
   - By: afterguard

4. **Gong Sound** (Downloaded: 21.1.2026)
   - Source: https://freesound.org/people/NoiseCollector/sounds/222922/
   - By: NoiseCollector

5. **Water Basin/Fountain** (Downloaded: 22.1.2026)
   - Source: https://freesound.org/people/morgantj/sounds/58574/
   - By: morgantj

6. **Fire/Lamp Sound** (Downloaded: 22.1.2026)
   - Source: https://freesound.org/people/hykenfreak/sounds/331621/
   - By: hykenfreak

7. **Toggle Off Sound** (Downloaded: 22.1.2026)
   - Source: https://freesound.org/people/Bee09/sounds/326561/
   - By: Bee09

8. **Bush Rustling** (Downloaded: 23.1.2026)
   - Source: https://freesound.org/people/elektroproleter/sounds/157567/
   - By: elektroproleter

## Setup Instructions
1. **Clone this repository:**
```bash
   git clone https://github.com/milamilovic/ZenGardenVR-public.git
   cd ZenGardenVR-public
```

2. **Download all required assets** from the sources listed above

3. **Place assets in their corresponding folders:**
   - The `.meta` files are already in place in this repository
   - Simply extract/copy the actual asset files into the folders alongside their `.meta` files
   - Unity will automatically recognize the assets using the existing `.meta` files
   - **Important:** Folder names must match exactly as specified

4. **Open project in Unity:**
   - Open Unity Hub
   - Click "Add" and select the project folder
   - Open the project

5. **Verify assets:**
   - Check the Console for any missing references
   - All `.meta` files should match with downloaded assets
   - If you see pink materials, you're missing textures/shaders

## Credits & Acknowledgments
### Unity Asset Store
- Nick Veselov - #NVJOB Water Shaders V2
- Nobiax / Yughues - Yughues Free Bushes
- A dog's life software - Textures
- Waldemarst - Japanese Garden Packages
- BG Studio / rpgwhitelock - Skybox
- Forge Creative - Japanese Zen Garden Pack
- Tobias Ungerboeck - Japanese Traditional Percussion
- Tanuki Digital - Shrine, Pagoda, and Lantern Packs
- logicalbeat - Shrine Pack
- Mikołaj Spychał - Mossy Rocks

### 3D Models (Sketchfab)
- Koi Fish model creator
- Gong model creator
- Lotus model creator
- Buddha Statue creator
- Tsukubai creator
- Bridge model creator
- Tea Table creator
- Geta shoes creator

### Textures (Polyhaven - CC0)
- Sand 03 texture
- Rocky Terrain 02 texture

### Audio (Freesound.org)
- FallujahQc - Footsteps
- JakLocke - Door sound
- afterguard - Water ambience
- NoiseCollector - Gong
- morgantj - Fountain
- hykenfreak - Fire
- Bee09 - Toggle sound
- elektroproleter - Bush rustling

## Author
Mila Milović
