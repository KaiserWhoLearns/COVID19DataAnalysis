import re
import pandas as pd
import pdb
# Read raw neighbor list
neighbors = pd.read_csv("neighbors_world_raw.csv")

# Change last token from . into \n
for idx, row in neighbors.iterrows():
    # Replace the content between : and \n with a comma
    neighbors["neighbor list"][idx] = str(neighbors["neighbor list"][idx]) + "\n"
    neighbors["neighbor list"][idx] = re.sub("[:].*[\n]", ",", neighbors["neighbor list"][idx])[:-1]

# Load the european countries list and print the ones that name does not match
with open("../territory_names/SouthAmerica_countries.txt", "r") as f:
    african_names = [line.rstrip() for line in f]

not_in_list = set()
te_names = set(neighbors["Country or territory"])
for name in african_names:
    if name not in te_names:
        not_in_list.add(name)

print(not_in_list)
# {'Cabo Verde', 'Congo (Kinshasa)', 'Gambia', 'Congo (Brazzaville)', 'Sao Tome and Principe', "Cote d'Ivoire"}
print(len(not_in_list))
# I manually changed the names
neighbors.to_csv("neighbors_world.csv")

