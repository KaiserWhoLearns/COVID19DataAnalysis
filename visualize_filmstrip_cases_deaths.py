import math

import matplotlib.pyplot as plt
import pandas
from matplotlib import cm
from matplotlib.colors import ListedColormap

from data import loader
from helpers import geomap, heatmap
import numpy


__NUMBER_OF_FILMS_IN_STRIP = 6


def roundup(num, nearest_ten=True):
    if nearest_ten:
        return int(math.ceil(num / 10.0)) * 10
    return int(math.ceil(num))


def plot_filmstrip(world, cases, deaths):
    countries_in_the_world = world['name'].to_list()
    country_wise_normalized_case_result = heatmap.process_heatmap_data(cases)
    country_wise_normalized_death_result = heatmap.process_heatmap_data(deaths)
    case_matrix = []
    death_matrix = []

    case_matrix_columns = [f'CaseStripMean{d+1}' for d in range(__NUMBER_OF_FILMS_IN_STRIP)]
    death_matrix_columns = [f'DeathStripMean{d+1}' for d in range(__NUMBER_OF_FILMS_IN_STRIP)]
    print(case_matrix_columns)
    print(death_matrix_columns)

    for country in countries_in_the_world:
        case_res = [None] * __NUMBER_OF_FILMS_IN_STRIP
        death_res = [None] * __NUMBER_OF_FILMS_IN_STRIP
        if country not in country_wise_normalized_case_result:
            # Both case_result and death_result have the same countries
            case_matrix.append(case_res)
            death_matrix.append(death_res)
            continue
        cases_count = country_wise_normalized_case_result[country]
        deaths_count = country_wise_normalized_death_result[country]
        case_film_strip_chunks = numpy.array_split(cases_count, __NUMBER_OF_FILMS_IN_STRIP)
        deaths_film_strip_chunks = numpy.array_split(deaths_count, __NUMBER_OF_FILMS_IN_STRIP)
        for index, chunk_data in enumerate(case_film_strip_chunks):
            case_res[index] = numpy.mean(chunk_data)
        for index, chunk_data in enumerate(deaths_film_strip_chunks):
            death_res[index] = numpy.mean(chunk_data)
        case_matrix.append(case_res)
        death_matrix.append(death_res)

    case_matrix_df = pandas.DataFrame(data=case_matrix, columns=case_matrix_columns)
    deaths_matrix_df = pandas.DataFrame(data=death_matrix, columns=death_matrix_columns)

    lets_concat = pandas.concat([world, case_matrix_df, deaths_matrix_df], axis=1)
    # lets_concat.drop('geometry', axis=1, inplace=True)

    calibrate_min = 0
    calibrate_case_max = roundup(case_matrix_df.max().max())
    calibrate_death_max = roundup(deaths_matrix_df.max().max(), nearest_ten=False)
    print(f'Cases: {calibrate_min} --> {calibrate_case_max}')
    print(f'Death: {calibrate_min} --> {calibrate_death_max}')

    fig, ax = plt.subplots(nrows=2, ncols=__NUMBER_OF_FILMS_IN_STRIP, figsize=(70, 20), sharex=True, sharey=True)
    case_palette_cmap = ListedColormap([cm.YlOrRd(x) for x in range(calibrate_case_max)])
    print(case_palette_cmap)
    death_palette_cmap = ListedColormap([cm.YlOrRd(x) for x in range(calibrate_death_max)])
    print(death_palette_cmap)
    # palette =
    # cmap = ListedColormap()
    # Plot all the cases first in the first row
    for column_index, filter_name in enumerate(case_matrix_columns):
        lets_concat.plot(
            column=filter_name,
            legend=True,
            ax=ax[0][column_index],
            vmin=calibrate_min,
            vmax=calibrate_case_max,
            cmap='viridis',
            missing_kwds={
                "color": "lightgrey",
                "edgecolor": "red",
                "hatch": "///",
                "label": "Missing values",
            },
            legend_kwds={
                'orientation': 'horizontal',
                'label': 'Cases per {}'.format(heatmap.get_normalized_per_population_value()),
            },
        )
        ax[0][column_index].set_axis_off()
    for column_index, filter_name in enumerate(death_matrix_columns):
        lets_concat.plot(
            column=filter_name,
            legend=True,
            ax=ax[1][column_index],
            cmap='viridis',
            vmin=calibrate_min,
            vmax=calibrate_death_max,
            missing_kwds={
                "color": "lightgrey",
                "edgecolor": "red",
                "hatch": "///",
                "label": "Missing values",
            },
            legend_kwds={
                'orientation': 'horizontal',
                'label': 'Deaths per {}'.format(heatmap.get_normalized_per_population_value()),
            },
        )
        ax[1][column_index].set_axis_off()
    plt.tight_layout()
    plt.savefig('results/filmstrip_world.pdf', bbox_inches='tight')


if __name__ == '__main__':
    cases, deaths = loader.get_global_case_and_deaths_time_series_data()
    world = geomap.get_world_data()
    world = world[(world.pop_est > 0) & (world.name != "Antarctica")]
    countries_in_the_world = set(world['name'].to_list())
    countries_in_covid_data = set(cases['Admin0'].to_list())
    print("Countries in GeoPandas: {}".format(len(countries_in_the_world)))
    print("Countries in Dataset  : {}".format(len(countries_in_covid_data)))
    common_countries = countries_in_covid_data.intersection(countries_in_the_world)
    diff_countries = countries_in_covid_data.difference(countries_in_the_world)
    print("Countries in Common: {}".format(len(common_countries)))
    print("Countries Not in Common: {}".format(len(diff_countries)))
    print(diff_countries)

    plot_filmstrip(world, cases, deaths)
