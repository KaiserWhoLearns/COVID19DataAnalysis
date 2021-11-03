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

# Read general data
df = pd.read_csv("owid-covid-data.csv", usecols=["location", "date", "cardiovasc_death_rate", "positive_rate", "people_vaccinated", "people_fully_vaccinated",
 "new_cases", "stringency_index", "new_deaths_smoothed", "new_tests_smoothed_per_thousand", "population_density"])
print(type(df["new_cases"]))

#%%
# countries = ["United States", "Italy", "India", "China"]
countries = ["United States", "Italy", "India"]
# Read data
for country in countries:
    country_data = df[df["location"] == country]
    country_data = country_data[country_data["people_vaccinated"] >= 0]

    ax1 = country_data.plot.scatter(x="people_fully_vaccinated", y="new_cases")
    ax1.set_title(country + " daily New Cases vs. People Fully Vaccinated")
    plt.show()

    ax1 = country_data.plot.scatter(x="people_vaccinated", y="new_cases")
    ax1.set_title(country + " daily New Cases vs. People Vaccinated")
    plt.show()

    ax1 = country_data.plot.scatter(x="people_vaccinated", y="new_deaths_smoothed")
    ax1.set_title(country + " daily Death vs. People Vaccinated")
    plt.show()

    ax1 = country_data.plot.scatter(x="stringency_index", y="new_deaths_smoothed")
    ax1.set_title(country + " daily Death vs. Stringency Index")
    plt.show()

    ax1 = country_data.plot.scatter(x="stringency_index", y="new_cases")
    ax1.set_title(country + " daily cases vs. Stringency Index")
    plt.show()

### Findings:
# 1. USA's data seems to have its own pattern compared to other countries. It may be better we look state by state
# 2. Although the new cases seems to decrease with vaccination count, there are a werid "peak" at the point of large vaccination count
#%%
countries = ["United States", "Italy", "India"]
# Read data
for country in countries:
    country_data = df[df["location"] == country]
    country_data = country_data[country_data["people_vaccinated"] >= 0]

    ax1 = country_data.plot.scatter(x="people_fully_vaccinated", y="positive_rate")
    ax1.set_title(country + " daily New Cases vs. Positive Rate per Test")
    plt.show()

# %%
### Gather features
from sklearn.cluster import KMeans
import numpy as np
import pdb
# Features: [max, min, median, mean] of [positive_rate, cardiovasc_death_rate, new_tests_smoothed_per_thousand, population_density, stringency index]
all_countries = df["location"]
all_countries.drop_duplicates(inplace=True)
columns = ["positive_rate", "cardiovasc_death_rate", "new_tests_smoothed_per_thousand", "population_density", "stringency_index"]
features = np.zeros([len(all_countries), 4 * len(columns)])
for country_idx, country in enumerate(all_countries):
    for idx, col in enumerate(columns):
        country_data = df[df["location"] == country]
        col_data = country_data[col]
        stats = col_data.describe()
        features[country_idx, idx] = stats["max"]
        features[country_idx, idx + len(columns)] = stats["min"]
        features[country_idx, idx + 2 * len(columns)] = stats["50%"]
        features[country_idx, idx + 3 * len(columns)] = stats["mean"]
print(features)

#%%
### Remove features with nan
clean_features = np.zeros([1, 4 * len(columns)])
clean_countries = []
for count in range(len(all_countries)):
    if np.sum(np.isnan(features[count])) == 0:
        clean_features = np.append(clean_features, np.array([features[count]]), axis=0)
        clean_countries.append(all_countries.iloc[count])
clean_features = clean_features[1:, :]
print(clean_features.shape)
print(len(clean_countries))
# %%
### Normalize data
means = np.mean(clean_features, axis=0)
stds = np.std(clean_features, axis=0)
normalized_features = (clean_features - means) / stds
print(means.shape)

### K-means
num_clus = 10
kmeans = KMeans(n_clusters=num_clus, random_state=0).fit(normalized_features)
labels_per_country = kmeans.labels_
print("Labels: ", labels_per_country)
print("Cluster centers", kmeans.cluster_centers_)
# %%
### Stats
groups = {}
for idx in range(len(labels_per_country)):
    if labels_per_country[idx] not in groups:
        groups[labels_per_country[idx]] = [clean_countries[idx]]
    else:
        groups[labels_per_country[idx]].append(clean_countries[idx])
print(groups)
# %%
