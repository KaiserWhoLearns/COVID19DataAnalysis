# EXISTING COLUMNS
# iso_code,continent,location,date,total_cases,new_cases,new_cases_smoothed,total_deaths,new_deaths,new_deaths_smoothed,total_cases_per_million,new_cases_per_million,
# new_cases_smoothed_per_million,total_deaths_per_million,new_deaths_per_million,new_deaths_smoothed_per_million,reproduction_rate,icu_patients,icu_patients_per_million,
# hosp_patients,hosp_patients_per_million,weekly_icu_admissions,weekly_icu_admissions_per_million,weekly_hosp_admissions,weekly_hosp_admissions_per_million,new_tests,
# total_tests,total_tests_per_thousand,new_tests_per_thousand,new_tests_smoothed,new_tests_smoothed_per_thousand,positive_rate,tests_per_case,tests_units,total_vaccinations,
# people_vaccinated,people_fully_vaccinated,total_boosters,new_vaccinations,new_vaccinations_smoothed,total_vaccinations_per_hundred,people_vaccinated_per_hundred,people_fully_vaccinated_per_hundred,
# total_boosters_per_hundred,new_vaccinations_smoothed_per_million,stringency_index,population,population_density,median_age,aged_65_older,aged_70_older,gdp_per_capita,
# extreme_poverty,cardiovasc_death_rate,diabetes_prevalence,female_smokers,male_smokers,handwashing_facilities,hospital_beds_per_thousand,life_expectancy,human_development_index,
# excess_mortality_cumulative_absolute,excess_mortality_cumulative,excess_mortality,excess_mortality_cumulative_per_million
#%%
from numpy.core.numeric import NaN
import pandas as pd
import matplotlib.pyplot as plt

# Read state data
confirm_by_state = pd.read_csv("data/UW time series/global 11-9-21/United States by county/US_state_confirmed_sm.csv")
death_by_state = pd.read_csv("data/UW time series/global 11-9-21/United States by county/US_state_deaths_sm.csv")

all_states = confirm_by_state["Admin1"]
with open("data/US_state_name.txt", "r") as f:
    all_states = [line.rstrip() for line in f]
# print(all_states)
#%%
# Process data and organize them into three month bucket
data_list = [confirm_by_state, death_by_state]
bucket_length = 90
total_days = len(death_by_state.columns) - 4

for idx, df in enumerate(data_list):
    # Remove regions that are not state
    df = df[df.Admin1.isin(all_states)]
    # Take sum for this datalist in length of bucket
    for bucket_idx in range(int(total_days / bucket_length) + 1):
        # print("Bucket " + str(bucket_idx))
        df['Bucket ' + str(bucket_idx)] = df.iloc[:, bucket_idx * bucket_length + 4: min((bucket_idx + 1) * bucket_length + 4, total_days + 4)].sum(axis=1)
        # print(df['Bucket ' + str(bucket_idx)])
    data_list[idx] = df
# %%
### Gather features
from sklearn.cluster import KMeans
import numpy as np
# Features: Bucket data from death and confirm cases
columns = ["Bucket " + str(bucket_idx) for bucket_idx in range(int(total_days / bucket_length) + 1)]
features = np.zeros([len(all_states), 2 * len(columns)])
for df_idx, df in enumerate(data_list):
    for state_idx, state in enumerate(all_states):
        for idx, col in enumerate(columns):
            state_data = df[df["Admin1"] == state]
            features[state_idx, idx + df_idx * len(columns)] = state_data[col]
# print(features)

# %%
### Normalize data
means = np.mean(features, axis=0)
stds = np.std(features, axis=0)
normalized_features = (features - means) / stds
print(means.shape)

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
import matplotlib.pyplot as plt
from mpl_toolkits.basemap import Basemap
from matplotlib.patches import Polygon

# create the map
map = Basemap(llcrnrlon=-119,llcrnrlat=22,urcrnrlon=-64,urcrnrlat=49,
        projection='lcc',lat_1=33,lat_2=45,lon_0=-95)

# load the shapefile, use the name 'states'
map.readshapefile('st99_d00', name='states', drawbounds=True)

# collect the state names from the shapefile attributes so we can
# look up the shape obect for a state by it's name
state_names = []
for shape_dict in map.states_info:
    state_names.append(shape_dict['NAME'])

ax = plt.gca() # get current axes instance

# get Texas and draw the filled polygon
seg = map.states[state_names.index('Texas')]
poly = Polygon(seg, facecolor='red',edgecolor='red')
ax.add_patch(poly)

plt.show()
# %%
