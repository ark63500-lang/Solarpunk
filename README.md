# Le Potager Solarpunk

Aux commandes d'un drone, on plante, on arrose et on récolte un potager avant la fin du chrono.

Projet réalisé avec Unity . Le jeu se lance depuis la scène 'Assets/Scenes/Menu.unity'.

Version jouable : https://zozito63.itch.io/solarpunk


## Le jeu

Le drone survole un potager vu de dessus et lâche des graines sur les rangées de terre. Chaque graine pousse toute seule : elle sort de terre, devient un légume mûr, puis pourrit si on attend trop puis elle explose. Un légume mûr se récolte avec le drone et rapporte des points. Un légume pourri finit par éclater et fait perdre des points. Arroser une plante la fait pousser plus vite.

Le drone dépend d'une antenne radio. S'il s'en éloigne trop, le signal baisse, puis le drone tombe et revient au point de départ.

La partie dure 3 minutes. Le meilleur score est gardé d'une session à l'autre.


## Contrôles

Au clavier :
- ZQSD ou les flèches pour se déplacer
- E pour lâcher une graine
- R (maintenu) pour arroser
- Espace (maintenu) pour récolter
- 1, 2, 3 pour choisir la graine
- Échap pour le menu pause

À la manette :
- Stick gauche pour se déplacer
- A pour lâcher une graine
- LT (maintenu) pour arroser
- RT (maintenu) pour récolter
- LB / RB ou la croix pour choisir la graine
- Start pour le menu pause


## Organisation du projet

Tout est rangé par type dans 'Assets/' : Animations, Audio, Commandes (Input Actions), Donnees, Materiaux, Modeles, Prefabs, Scenes, Scripts, Textures.

Les principaux réglages se font dans l'Inspector :
- la vitesse du drone sur le drone, composant 'DronePilotage'
- les durées de croissance et les points de chaque légume dans 'Assets/Donnees/Plantes'
- le bonus d'arrosage sur le drone, composant 'DroneArrosage'
- la durée de la partie sur l'objet 'Partie'
- les sons et volumes sur l'objet 'Audio'

Côté code, le script 'Partie' garde tout l'état de la partie (score, chrono, plantes, récoltes). Les autres scripts reçoivent une référence vers elle plutôt que de passer par des variables globales. Les sons et l'interface réagissent aux événements de la partie. Le meilleur score est sauvegardé avec PlayerPrefs.


## Ressources utilisées

Le cours et sa fiche projet, ainsi que la documentation officielle de Unity (Input System, Prefabs, Coroutines, Collisions, Animator, Audio, Particules, UI, Scènes, PlayerPrefs, Build).

Chaînes les plus suivies pour les tutos :
- TUTO UNITY FR
- Code Monkey
- Jimmy Vegas

Et sans doute bien d'autres dont j'ai oublié de noter le nom. J'ai aussi beaucoup utilisé les forums, comme Unity Discussions ou Unity3D sur Reddit.


## Utilisation de l'IA

J'ai aussi utilisé l'IA comme aide pour certains bouts de code, et surtout pour le peaufinage et la lisibilité du code, ainsi que comme correcteur sur des textes pour éviter les fautes. L'IA m'a surtout aidé à trouver ce que je n'arrivais pas à trouver sur YouTube ou sur les forums.


## Crédits

Modèles et décor (Unity Asset Store) :
- Low Poly Village 
- Ultimate Nature  Starter, par Innerverse Interactive 
- Rocks Stylized, par PolyOne
- Stylized Water Effects, par NamuFX 
- EzTornado 

Sons :
- Grenade Sound FX (Unity Asset Store)
- Bruit de tuyau d'arrosage, par alex_jauk (Pixabay)
- Musique d'ambiance 
- Autres effets sonores 
