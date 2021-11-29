#%%
from data import loader
from helpers import heatmap
import pdb
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

# d1 = loader.get_global_case_and_deaths_time_series_data()
# # d2 = loader.get_continent_specific_case_and_deaths_time_series_data(continent=<>)
# d3 = loader.get_available_and_supported_continents()  # In case you want to see continents supported to pass into the call above
# d = loader.get_united_states_case_and_death_time_series_data(county=True)  # True if you want State + County, and False if you want only State and not county level.

# death_data = d[1]
# case_count_data = d[0]


countries = ["US", "Italy", "India"]
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
    plt.imshow(death_similarity)
    plt.colorbar()

    ax = plt.gca()
    # xticks = all_states
    # yticks = all_states
    # ax.xaxis.set_xticks(xticks)
    # ax.xaxis.set_yticks(yticks)
    plt.title("Death cosine similarity for " + country)
    plt.show()

    plt.imshow(case_similarity)
    plt.colorbar()

    ax = plt.gca()
    # xticks = all_states
    # yticks = all_states
    # ax.xaxis.set_xticks(xticks)
    # ax.xaxis.set_yticks(yticks)
    plt.title("Case cosine similarity for " + country)
    plt.show()


# %%
