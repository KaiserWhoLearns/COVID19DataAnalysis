#%%

# Compute the similarity scores between countries/counties
# * Assumption: neighboring zones should have more similar pattern in day-to-day pattern because of the transmission
from data import loader
from helpers import heatmap
import pdb
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

import matplotlib as mpl
mpl.rcParams['figure.dpi'] = 1000

d1 = loader.get_global_case_and_deaths_time_series_data()
# d2 = loader.get_continent_specific_case_and_deaths_time_series_data(continent=<'Africa'>)
d3 = loader.get_available_and_supported_continents()  # In case you want to see continents supported to pass into the call above
d = loader.get_united_states_case_and_death_time_series_data(county=True)  # True if you want State + County, and False if you want only State and not county level.

pdb.set_trace()
# death_data = d[1]
# case_count_data = d[0]

#%%

def plot_sim_scores(sim_matrix, terr_names, data_type, continent="Africa"):
    # Plot the similarity scores between given country
    if len(terr_names) > 35:
        label_size = 5
    else:
        label_size = 6

    plt.imshow(sim_matrix, vmin=0, vmax=1)
    plt.colorbar()

    ax = plt.gca()

    ax.set_xticks([i for i in range(len(terr_names))])
    ax.set_xticklabels(terr_names, Rotation=90)

    ax.set_yticks([i for i in range(len(terr_names))])
    ax.set_yticklabels(terr_names)
    ax.tick_params(axis='both', which='major', labelsize=label_size)
    plt.title(data_type + " cosine similarity near " + terr_names[0])
    plt.savefig("results/similarity scores/" + continent + "/" + data_type + "_" + terr_names[0] + ".png")
    plt.clf()
    # plt.show()

continent = "Europe"
# Generate plot for neighboring African countries
d2 = loader.get_continent_specific_case_and_deaths_time_series_data(continent=continent)



with open("data/territory_names/" + continent + "_countries.txt", "r") as f:
    country_names = [line.rstrip() for line in f]

neighbors = pd.read_csv("data/neighbor_map/neighbors_world.csv")

data_names = ["Case Count", "Death Count"]

# For each african country, generate the plot in the form of country - neighbors - casecount 0
for df_idx, df in enumerate(d2):
    total_days = np.array(["Day" in x for x in df.columns]).sum()
    columns = ["Day " + str(day) for day in range(total_days)]
    for base_country in df["Admin0"]:
        # Create a matrix to store processed data [country + neighbors, columns]
        if type(neighbors.loc[neighbors["Country or territory"]==base_country]["neighbor list"].tolist()[0]) is str:
            list_of_neighbors = neighbors.loc[neighbors["Country or territory"]==base_country]["neighbor list"].tolist()[0].split(",")
        else:
            pass
        list_of_neighbors.insert(0, base_country)
        # Remove the neighbors not in the list and leading white space
        actual_list = []
        for idx in range(len(list_of_neighbors)):
            list_of_neighbors[idx] = list_of_neighbors[idx].lstrip()
            if list_of_neighbors[idx] in country_names:
                actual_list.append(list_of_neighbors[idx])
        list_of_neighbors = actual_list

        processed_count = np.zeros([len(list_of_neighbors) + 1, len(columns)])
        for ctry_idx, country in enumerate(list_of_neighbors):
            for idx, col in enumerate(columns):
                processed_count[ctry_idx, idx] = df[df["Admin0"]==country][col].values

        sim_matrix = np.zeros([len(list_of_neighbors), len(list_of_neighbors)])
        for i in range(len(list_of_neighbors)):
            for j in range(len(list_of_neighbors)):
                sim_matrix[i, j] = processed_count[i, :].dot(processed_count[j, :])/ (np.linalg.norm(processed_count[i, :]) * np.linalg.norm(processed_count[j, :]))

        plot_sim_scores(sim_matrix, list_of_neighbors, data_names[df_idx], continent)

#%%

pdb.set_trace()

countries = ["US", "Italy", "India", "Russia"]
for country in countries:
    # Read state data
    confirm_by_state = pd.read_csv("data/UW time series/Global/World by province/" + country + "_confirmed_sm.csv")
    death_by_state = pd.read_csv("data/UW time series/Global/World by province/" + country + "_deaths_sm.csv")

    all_states = confirm_by_state["Admin1"]
    if country == "US":
        with open("data/US_state_name.txt", "r") as f:
            all_states = [line.rstrip() for line in f]
    # print(all_states)

    # Process data and organize them into three month bucket
    data_list = [confirm_by_state, death_by_state]
    bucket_length = 30
    total_days = len(death_by_state.columns) - 7

    columns = ["Day " + str(day) for day in range(total_days)]
    death_count = np.zeros([len(all_states), len(columns)])
    case_count = np.zeros([len(all_states), len(columns)])
    for df_idx, df in enumerate(data_list):
        for state_idx, state in enumerate(all_states):
            for idx, col in enumerate(columns):
                state_data = df[df["Admin1"] == state]
                if df_idx == 1:
                    death_count[state_idx, idx] = state_data[col] / state_data["Population"] * 1000000
                else:
                    case_count[state_idx, idx] = state_data[col] / state_data["Population"] * 1000000
                # features[state_idx, idx + df_idx * len(columns)] = state_data[col] / state_data["Population"] * 1000000
    # print(features)

    case_similarity = np.zeros([len(all_states), len(all_states)])
    death_similarity = np.zeros([len(all_states), len(all_states)])
    for i in range(len(all_states)):
        for j in range(len(all_states)):
            case_similarity[i, j] = case_count[i, :].dot(case_count[j, :])/ (np.linalg.norm(case_count[i, :]) * np.linalg.norm(case_count[j, :]))
            death_similarity[i, j] = death_count[i, :].dot(death_count[j, :])/ (np.linalg.norm(death_count[i, :]) * np.linalg.norm(death_count[j, :]))
    print(death_similarity)
    print(case_similarity)
    #%%
    # Plotting

    if len(all_states) > 35:
        label_size = 5
    else:
        label_size = 6


    plt.imshow(death_similarity, vmin=0, vmax=1)
    plt.colorbar()

    ax = plt.gca()
    # xticks = all_states
    # yticks = all_states
    # ax.xaxis.set_xticks(xticks)
    # ax.xaxis.set_yticks(yticks)

    ax.set_xticks([i for i in range(len(all_states))])
    ax.set_xticklabels(all_states, Rotation=90)

    ax.set_yticks([i for i in range(len(all_states))])
    ax.set_yticklabels(all_states)
    ax.tick_params(axis='both', which='major', labelsize=label_size)
    plt.title("Death cosine similarity for " + country)
    plt.show()

    plt.imshow(case_similarity, vmin=0, vmax=1)
    plt.colorbar()

    ax = plt.gca()
    # xticks = all_states
    # yticks = all_states
    # ax.xaxis.set_xticks(xticks)
    # ax.xaxis.set_yticks(yticks)

    ax.set_xticks([i for i in range(len(all_states))])
    ax.set_xticklabels(all_states, Rotation=90)

    ax.set_yticks([i for i in range(len(all_states))])
    ax.set_yticklabels(all_states)

    ax.tick_params(axis='both', which='major', labelsize=label_size)
    plt.title("Case cosine similarity for " + country)
    plt.show()

# %%
