import torch

checkpoint = torch.load(
    'results/training024/AgentRealisticBehavior/checkpoint.pt', 
    map_location=torch.device('cpu')
)

torch.save(checkpoint, 'results/training024/AgentRealisticBehavior/checkpoint.pt')

print("Done!")