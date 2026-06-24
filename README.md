# Machine Learning Transfert Research : Realist Prototype
Prototype for project for the research project: **Evaluating transfer learning for reinforcement learning agents across same-genre video games**

Research project carried out in partnership between [PulluP Entertainment](https://pullupent.com/en) and [ISART Digital Paris](https://www.isart.fr/)

## Presentation 
This project has been realized on **Unity 6.3.9f1** with the team of forth to prototype a realist racing  gameplay in order to train on it, an agent with machine learning and transfert it on the other prototype we made, an [Arcade prototype](https://github.com/Machine-Learning-Transfert-Research/Arcade-Prototype).

To realize this prototype, we use multiple plugins to help us on the production : 
- **ML-Agents** 
- **Vehicule Physic Pro**

## How to start the project
1. From Unity Hub, add project from disk and select the main folder of the project
2. Launch the project

### Launch a training
---
Warning : Make sure to have install the python environemment

1. Open the scene **Training** in **Unity**
2. Open **MiniForge Prompt** in the folder of the project
2. Write this command to activate the python environnement
``` 
activate conda mlagents
```
3. Launch the traning with this command :
```
mlagents-learn config/AgentRealisticBehavior_config.yaml --run-id=Training_Name
```
4. **Launch** the scene

### Test agents trained
---
1. Open the scene **MainCircuit** in **Unity**
2. Get an **.onnx** file from the folder **Result** in the **Assets/Results**
3. On the car pickup, in the component Behavior Parameters 
    - Add the **.onnx** in the variable model 
    - Change the Behavior Type to Inference only

4. **Launch** the scene

### Play agent
---
1. Open either the scene **MainCircuit** or **Training**
2. On the car pickup, in the component Behavior Parameters 
    - Change the Behavior Type to Heuristic onlys


## Credits
- [Bryan BACHELET](https://www.linkedin.com/in/bryan-bachelet/)
- [Vincent DEVINE](https://www.linkedin.com/in/vincent-devine/)
- [Matéo ERBISTI](https://www.linkedin.com/in/mat%C3%A9o-erbisti/)
- [Omaya LISE](https://www.linkedin.com/in/omaya-lise/)
- [Aurelien CHAMBON](https://www.linkedin.com/in/aurelien-chambon/)
- [Aurélien LHERBIER](https://www.linkedin.com/in/aur%C3%A9lien-lherbier-a344993b/)

