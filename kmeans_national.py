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
confirm_by_state = pd.read_csv("data/UW time series/Global/United States by county/US_state_confirmed_sm.csv")
death_by_state = pd.read_csv("data/UW time series/Global/United States by county/US_state_deaths_sm.csv")

all_states = confirm_by_state["Admin1"]
with open("data/US_state_name.txt", "r") as f:
    all_states = [line.rstrip() for line in f]
# print(all_states)
#%%
# Process data and organize them into three month bucket
data_list = [confirm_by_state, death_by_state]
bucket_length = 30
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
result_path = "results/US_state_clusters.csv"
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
