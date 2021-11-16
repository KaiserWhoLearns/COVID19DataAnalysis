from helpers import geomap
import matplotlib.pyplot as plt


def example_usa_states_continental_plot():
    states = geomap.get_usa_state_data()
    fig, ax = plt.subplots(nrows=1, ncols=1, figsize=(20, 10))
    ax = states.plot(cmap='magma')
    ax.set_xticks([])
    ax.set_yticks([])
    plt.savefig('results/helper_geomap_results/usa_mainland.png', bbox_inches='tight')
    plt.close('all')


def example_usa_states_all_plot():
    states = geomap.get_usa_state_data(mainland_only=False)  # Set this to false
    states = states.set_index('STUSPS').drop(index=[
        'VI',  # US Virgin Islands
        'MP',  # Northern Mariana Islands
        'AS',  # American Samoa
    ])

    fig, continental_ax = plt.subplots(figsize=(20, 10))
    alaska_ax = continental_ax.inset_axes([.08, .01, .20, .28])
    hawaii_ax = continental_ax.inset_axes([.28, .01, .15, .19])
    p_rico_ax = continental_ax.inset_axes([.43, .01, .10, .15])
    guam_ax = continental_ax.inset_axes([.60, .01, .15, .10])

    continental_ax.set_xlim(-130, -64)
    continental_ax.set_ylim(22, 53)

    alaska_ax.set_ylim(51, 72)
    alaska_ax.set_xlim(-180, -127)

    hawaii_ax.set_ylim(18.8, 22.5)
    hawaii_ax.set_xlim(-160, -154.6)

    vmin, vmax = states['ALAND'].agg(['min', 'max'])
    states.drop(index=['HI', 'AK']).plot(cmap='magma', ax=continental_ax, vmin=vmin, vmax=vmax)
    states.loc[['AK']].plot(column="ALAND", ax=alaska_ax, vmin=vmin, vmax=vmax)
    states.loc[['HI']].plot(column="ALAND", ax=hawaii_ax, vmin=vmin, vmax=vmax)
    states.loc[['PR']].plot(column="ALAND", ax=p_rico_ax, vmin=vmin, vmax=vmax)
    states.loc[['GU']].plot(column="ALAND", ax=guam_ax, vmin=vmin, vmax=vmax)

    # remove ticks
    for ax in [continental_ax, alaska_ax, hawaii_ax, p_rico_ax, guam_ax]:
        ax.set_yticks([])
        ax.set_xticks([])

    plt.savefig('results/helper_geomap_results/usa_incl_ak_hi_pr_gu.png', bbox_inches='tight')
    plt.close('all')


if __name__ == '__main__':
    example_usa_states_continental_plot()
    example_usa_states_all_plot()
