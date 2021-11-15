#%%
from numpy.core.numeric import NaN
import pandas as pd
import matplotlib.pyplot as plt

# Read state data
peak_data = pd.read_csv("data/UW time series/Global/Peak Sets/UnitedStates.csv")

all_states = peak_data["Admin1"]
num_of_peaks = 9
# print(all_states)
print(peak_data.shape)
# %%
### Gather features
from sklearn.cluster import KMeans
import numpy as np
# Features: Bucket data from death and confirm cases
features = np.zeros([len(all_states), peak_data.shape[1] - 5])

for state_idx, state in enumerate(all_states):
    state_data = peak_data[peak_data["Admin1"] == state]
    for peak_idx in range(1, 10):
        # Record days
        features[state_idx, (peak_idx - 1) * 2] = state_data["X" + str(peak_idx)]
        # Record cases
        features[state_idx, (peak_idx - 1) * 2 + 1] = state_data["Y" + str(peak_idx)] / state_data["Population"] * 1000000
# print(features)

# %%
### Normalize data
means = np.mean(features, axis=0)
stds = np.std(features, axis=0)
# Add mini number to avoid divide by 0
stds += 0.00001 * np.ones(stds.shape)
normalized_features = (features - means) / stds

### K-means
num_clus = 10
kmeans = KMeans(n_clusters=num_clus, random_state=0).fit(normalized_features)
labels_per_country = kmeans.labels_
print("Labels: ", labels_per_country)
# print("Cluster centers", kmeans.cluster_centers_)
# %%
### Stats
groups = {}
for idx in range(len(labels_per_country)):
    if labels_per_country[idx] not in groups:
        groups[labels_per_country[idx]] = [all_states[idx]]
    else:
        groups[labels_per_country[idx]].append(all_states[idx])
print(groups)
# %%
# Write results to CSV
import csv
result_path = "results/US_state_clusters_by_peak.csv"
with open(result_path, mode='w') as csv_file:
    writer = csv.writer(csv_file, delimiter=',', quotechar='"', quoting=csv.QUOTE_MINIMAL)

    writer.writerow(['state', 'class'])
    for label in groups:
        for state in groups[label]:
            writer.writerow([state, label])
#%%
import altair as alt
from vega_datasets import data

# Get id for states
ansi = pd.read_csv('https://www2.census.gov/geo/docs/reference/state.txt', sep='|')
ansi.columns = ['id', 'abbr', 'state', 'statens']
ansi = ansi[['id', 'abbr', 'state']]

state_labels = pd.read_csv(result_path)
state_labels = pd.merge(state_labels, ansi, how='left', on='state')

states = alt.topo_feature(data.us_10m.url, 'states')
source = data.us_state_capitals.url
# pdb.set_trace()
alt.Chart(states).mark_geoshape().encode(
    color='class:Q'
).transform_lookup(
    lookup='id',
    from_=alt.LookupData(state_labels, 'id', ['class'])
).project(
    type='albersUsa'
).properties(
    width=500,
    height=300
)
# %%
